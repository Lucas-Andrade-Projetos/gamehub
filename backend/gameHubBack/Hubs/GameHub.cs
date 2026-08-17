using System.Security.Claims;
using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.DTOs.Rooms;
using gameHubBack.Interfaces;
using gameHubBack.Services;
using gameHubBack.Services.BatalhaRural;
using gameHubBack.Services.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace gameHubBack.Hubs;

[Authorize]
public class GameHub(
    IRoomRegistry roomRegistry,
    IBatalhaRuralGameService gameService,
    DelayedActionScheduler scheduler,
    IHubContext<GameHub> hubContext,
    IServiceScopeFactory scopeFactory) : Hub
{
    private const string RoomBrowserGroup = "room-browser";
    private static readonly TimeSpan ReconnectGrace = TimeSpan.FromSeconds(30);

    public async Task<string> CreateRoom()
    {
        var (userId, nickname) = GetIdentity();

        var room = roomRegistry.CreateRoom(userId, nickname, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await Clients.Caller.SendAsync("RoomUpdated", ToStateDto(room));
        await BroadcastOpenRooms();

        return room.Code;
    }

    public async Task JoinRoom(string code)
    {
        var (userId, nickname) = GetIdentity();

        var room = roomRegistry.TryJoinRoom(code.ToUpperInvariant(), userId, nickname, Context.ConnectionId);

        if (room == null) throw new HubException("Sala não encontrada ou cheia");

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));
        await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} entrou na sala"));
        await BroadcastOpenRooms();
    }

    public async Task<RoomStateDto?> Rejoin(string code)
    {
        var (userId, nickname) = GetIdentity();

        var room = roomRegistry.TryRejoin(code.ToUpperInvariant(), userId, Context.ConnectionId);

        if (room == null) return null;

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);

        if (room.GameId != null)
        {
            scheduler.Cancel(AbandonTimerKey(room.GameId, userId));
            await gameService.RefreshTurnDeadlineAsync(room.GameId);
            ScheduleTurnTimer(room.GameId, room.Code);
        }

        await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));
        await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} reconectou"));

        return ToStateDto(room);
    }

    public async Task SendMessage(string code, string text)
    {
        var (_, nickname) = GetIdentity();

        await Clients.Group(code).SendAsync("ChatMessageReceived", new ChatMessageDto
        {
            Nickname = nickname,
            Text = text,
            SentAt = DateTime.UtcNow
        });
    }

    public async Task SetReady(string code, bool ready)
    {
        var (userId, _) = GetIdentity();

        var bothReady = roomRegistry.SetReady(code, userId, ready);
        var room = roomRegistry.GetRoom(code);

        if (room == null) return;

        await Clients.Group(code).SendAsync("RoomUpdated", ToStateDto(room));

        if (bothReady && room.Player1 != null && room.Player2 != null)
        {
            var game = await gameService.CreateGameAsync(
                room.Player1.UserId, room.Player1.Nickname,
                room.Player2.UserId, room.Player2.Nickname);

            roomRegistry.SetGameId(code, game.Id);

            await Clients.Group(code).SendAsync("GameReady", game.Id);
        }
    }

    public Task<List<RoomSummaryDto>> ListOpenRooms()
    {
        return Task.FromResult(roomRegistry.GetOpenRooms().Select(ToSummaryDto).ToList());
    }

    public async Task NotifyPlacementReady(string gameId, string code)
    {
        var (userId, _) = GetIdentity();

        var bothReady = await gameService.SetPlayerReadyAsync(gameId, userId);

        if (bothReady)
        {
            await Clients.Group(code).SendAsync("BattleStarting");
            ScheduleTurnTimer(gameId, code);
        }
    }

    public async Task Attack(string gameId, string code, int x, int y)
    {
        var (userId, nickname) = GetIdentity();

        var result = await gameService.AttackAsync(gameId, userId, x, y);

        if (result == null) return;

        await Clients.Group(code).SendAsync("AttackResolved", result);

        if (result.Success)
        {
            var shotText = result.Hit
                ? $"{nickname} disparou em (X {result.X + 1}, Y {result.Y + 1}) e estourou o coitado!"
                : $"{nickname} disparou em (X {result.X + 1}, Y {result.Y + 1}), mas não acertou nada...";

            await Clients.Group(code).SendAsync("ChatMessageReceived", SystemMessage(shotText));
        }

        if (result.GameEnded && result.WinnerPlayerNum != null)
        {
            scheduler.Cancel(TurnTimerKey(gameId));
            await Clients.Group(code).SendAsync("GameEnded", new GameEndedDto
            {
                WinnerPlayerNum = result.WinnerPlayerNum.Value,
                Reason = "Normal"
            });
        }
        else if (result.Success)
        {
            ScheduleTurnTimer(gameId, code);
        }
    }

    public async Task ReturnToRoom(string code)
    {
        var room = roomRegistry.ResetRoom(code);

        if (room != null) await Clients.Group(code).SendAsync("RoomUpdated", ToStateDto(room));
    }

    public async Task LeaveRoom(string code)
    {
        var (_, nickname) = GetIdentity();

        var room = roomRegistry.HandleDisconnect(Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);

        if (room != null)
        {
            await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));
            await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} saiu da sala"));
        }

        await BroadcastOpenRooms();
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomBrowserGroup);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var nickname = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var room = roomRegistry.HandleDisconnect(Context.ConnectionId);

        if (room != null)
        {
            await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));

            if (room.GameId != null)
            {
                var gameId = room.GameId;
                var code = room.Code;

                scheduler.Cancel(TurnTimerKey(gameId));

                if (nickname != null)
                {
                    await Clients.Group(code).SendAsync(
                        "ChatMessageReceived",
                        SystemMessage($"{nickname} caiu da partida. Aguardando reconexão..."));
                }

                if (userId != null) ScheduleAbandonTimer(gameId, code, userId);
            }
            else if (nickname != null)
            {
                await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} saiu da sala"));
            }
        }

        await BroadcastOpenRooms();
        await base.OnDisconnectedAsync(exception);
    }

    private void ScheduleTurnTimer(string gameId, string code)
    {
        scheduler.Schedule(TurnTimerKey(gameId), GameService.TurnDuration, async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var scopedGameService = scope.ServiceProvider.GetRequiredService<IBatalhaRuralGameService>();

            var result = await scopedGameService.TimeoutTurnAsync(gameId);

            if (result == null) return;

            await hubContext.Clients.Group(code).SendAsync("TurnTimedOut", result);
            ScheduleTurnTimer(gameId, code);
        });
    }

    private void ScheduleAbandonTimer(string gameId, string code, string disconnectedUserId)
    {
        scheduler.Schedule(AbandonTimerKey(gameId, disconnectedUserId), ReconnectGrace, async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var scopedGameService = scope.ServiceProvider.GetRequiredService<IBatalhaRuralGameService>();

            var result = await scopedGameService.ForfeitAsync(gameId, disconnectedUserId);

            if (result?.WinnerPlayerNum == null) return;

            await hubContext.Clients.Group(code).SendAsync("GameEnded", new GameEndedDto
            {
                WinnerPlayerNum = result.WinnerPlayerNum.Value,
                Reason = "Abandonment"
            });
        });
    }

    private static string TurnTimerKey(string gameId) => $"turn:{gameId}";

    private static string AbandonTimerKey(string gameId, string userId) => $"abandon:{gameId}:{userId}";

    private static ChatMessageDto SystemMessage(string text)
    {
        return new ChatMessageDto { Nickname = "Sistema", Text = text, SentAt = DateTime.UtcNow, IsSystem = true };
    }

    private Task BroadcastOpenRooms()
    {
        return Clients.Group(RoomBrowserGroup).SendAsync("RoomListUpdated", roomRegistry.GetOpenRooms().Select(ToSummaryDto));
    }

    private (string UserId, string Nickname) GetIdentity()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("Usuário não autenticado");
        var nickname = Context.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? throw new HubException("Usuário não autenticado");

        return (userId, nickname);
    }

    private static RoomStateDto ToStateDto(GameRoom room)
    {
        return new RoomStateDto
        {
            Code = room.Code,
            GameId = room.GameId,
            Player1 = room.Player1 == null
                ? null
                : new RoomPlayerDto { UserId = room.Player1.UserId, Nickname = room.Player1.Nickname, Ready = room.Player1.Ready, Connected = room.Player1.Connected },
            Player2 = room.Player2 == null
                ? null
                : new RoomPlayerDto { UserId = room.Player2.UserId, Nickname = room.Player2.Nickname, Ready = room.Player2.Ready, Connected = room.Player2.Connected }
        };
    }

    private static RoomSummaryDto ToSummaryDto(GameRoom room)
    {
        return new RoomSummaryDto
        {
            Code = room.Code,
            HostNickname = room.Player1?.Nickname ?? room.Player2?.Nickname ?? "?"
        };
    }
}
