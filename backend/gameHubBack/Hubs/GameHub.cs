using System.Security.Claims;
using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.DTOs.Rooms;
using gameHubBack.Enums.BatalhaRural;
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
    BatalhaRuralTimings timings,
    IHubContext<GameHub> hubContext,
    IServiceScopeFactory scopeFactory) : Hub
{
    private const string RoomBrowserGroup = "room-browser";
    private const int MaxMessageLength = 300;

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
            scheduler.Cancel(RoomCleanupKey(room.Code));

            var deadline = await gameService.RefreshTurnDeadlineAsync(room.GameId, userId);

            if (deadline != null) ScheduleTurnTimer(room.GameId, room.Code, deadline.Value - DateTime.UtcNow);
        }

        await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));
        await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} reconectou"));

        return ToStateDto(room);
    }

    public async Task SendMessage(string code, string text)
    {
        var (_, nickname) = GetIdentity();
        var room = RequireCallerRoom(code);

        text = text?.Trim() ?? "";
        if (text.Length == 0) return;
        if (text.Length > MaxMessageLength) text = text[..MaxMessageLength];

        await Clients.Group(room.Code).SendAsync("ChatMessageReceived", new ChatMessageDto
        {
            Nickname = nickname,
            Text = text,
            SentAt = DateTime.UtcNow
        });
    }

    public async Task SetReady(string code, bool ready)
    {
        var (userId, _) = GetIdentity();
        var room = RequireCallerRoom(code);

        if (room.GameId != null) throw new HubException("A partida já começou");

        var bothReady = roomRegistry.SetReady(room.Code, userId, ready);

        await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));

        if (bothReady) await StartGameAsync(room);
    }

    public async Task RequestRematch(string code)
    {
        var (userId, nickname) = GetIdentity();
        var room = RequireCallerRoom(code);

        if (room.GameId == null) throw new HubException("Não há partida para revanche");

        var game = await gameService.GetGameAsync(room.GameId);

        if (game == null || game.Status != GameStatus.Ended) throw new HubException("A partida ainda não terminou");

        if (room.Player1?.Connected != true || room.Player2?.Connected != true)
            throw new HubException("O adversário saiu da sala");

        var bothWant = roomRegistry.RequestRematch(room.Code, userId);

        if (!bothWant)
        {
            await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));
            await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} quer revanche!"));
            return;
        }

        // no RoomUpdated in between: clients would see GameId = null for a moment and think
        // the room went back to the lobby. StartGameAsync broadcasts once the new game is set.
        roomRegistry.ResetRoom(room.Code);
        await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage("Revanche aceita!"));
        await StartGameAsync(room);
    }

    public Task<List<RoomSummaryDto>> ListOpenRooms()
    {
        return Task.FromResult(roomRegistry.GetOpenRooms().Select(ToSummaryDto).ToList());
    }

    public async Task NotifyPlacementReady(string gameId, string code)
    {
        var (userId, _) = GetIdentity();
        code = RequireCallerGame(code, gameId).Code;

        var result = await gameService.SetPlayerReadyAsync(gameId, userId);

        if (result == ReadyResult.NotAllowed)
            throw new HubException("Posicione todas as peças antes de confirmar");

        if (result == ReadyResult.BothReady)
        {
            scheduler.Cancel(PlacementTimerKey(gameId));
            await Clients.Group(code).SendAsync("BattleStarting");
            ScheduleTurnTimer(gameId, code);
        }
    }

    public async Task Attack(string gameId, string code, int x, int y)
    {
        var (userId, nickname) = GetIdentity();
        code = RequireCallerGame(code, gameId).Code;

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
        code = RequireCallerRoom(code).Code;
        var currentGameId = roomRegistry.GetRoom(code)?.GameId;

        if (currentGameId != null)
        {
            var game = await gameService.GetGameAsync(currentGameId);

            if (game != null && game.Status is not (GameStatus.Ended or GameStatus.Cancelled))
                throw new HubException("A partida ainda está em andamento");
        }

        var room = roomRegistry.ResetRoom(code);

        if (room != null) await Clients.Group(code).SendAsync("RoomUpdated", ToStateDto(room));
    }

    public async Task LeaveRoom(string code)
    {
        var (userId, nickname) = GetIdentity();
        var callerRoom = roomRegistry.GetRoomByConnection(Context.ConnectionId);

        // leaving on purpose mid-match is an immediate forfeit - otherwise the player would
        // just be marked disconnected with no abandon timer and the match would never end
        if (callerRoom?.GameId != null)
        {
            var gameId = callerRoom.GameId;
            var result = await gameService.ForfeitAsync(gameId, userId);

            if (result?.WinnerPlayerNum != null)
            {
                CancelGameTimers(gameId);
                await Clients.Group(callerRoom.Code).SendAsync("GameEnded", new GameEndedDto
                {
                    WinnerPlayerNum = result.WinnerPlayerNum.Value,
                    Reason = "Abandonment"
                });
            }
        }

        var room = roomRegistry.HandleDisconnect(Context.ConnectionId);

        if (callerRoom != null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, callerRoom.Code);

        if (room != null)
        {
            await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));
            await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} saiu da sala"));

            if (room.GameId != null && !HasConnectedPlayers(room)) ScheduleRoomCleanup(room.Code);
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

                // the turn clock pauses while someone is away; the placement clock doesn't
                scheduler.Cancel(TurnTimerKey(gameId));

                if (nickname != null)
                {
                    await Clients.Group(code).SendAsync(
                        "ChatMessageReceived",
                        SystemMessage($"{nickname} caiu da partida. Aguardando reconexão..."));
                }

                if (userId != null) ScheduleAbandonTimer(gameId, code, userId);

                if (!HasConnectedPlayers(room)) ScheduleRoomCleanup(code);
            }
            else if (nickname != null)
            {
                await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} saiu da sala"));
            }
        }

        await BroadcastOpenRooms();
        await base.OnDisconnectedAsync(exception);
    }

    private async Task StartGameAsync(GameRoom room)
    {
        if (room.Player1 == null || room.Player2 == null) return;

        var game = await gameService.CreateGameAsync(
            room.Player1.UserId, room.Player1.Nickname,
            room.Player2.UserId, room.Player2.Nickname);

        roomRegistry.SetGameId(room.Code, game.Id);

        await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));
        await Clients.Group(room.Code).SendAsync("GameReady", game.Id);

        SchedulePlacementTimer(game.Id, room.Code);
    }

    private void SchedulePlacementTimer(string gameId, string code)
    {
        scheduler.Schedule(PlacementTimerKey(gameId), timings.PlacementDuration, async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var scopedGameService = scope.ServiceProvider.GetRequiredService<IBatalhaRuralGameService>();

            var autoPlaced = await scopedGameService.AutoCompletePlacementAsync(gameId);

            if (autoPlaced == null) return;

            if (autoPlaced.Count > 0)
            {
                await hubContext.Clients.Group(code).SendAsync("ChatMessageReceived", SystemMessage(
                    $"Tempo de posicionamento esgotado! Peças posicionadas aleatoriamente para: {string.Join(", ", autoPlaced)}."));
            }

            await hubContext.Clients.Group(code).SendAsync("BattleStarting");
            ScheduleTurnTimer(gameId, code);
        });
    }

    private void ScheduleTurnTimer(string gameId, string code, TimeSpan? delay = null)
    {
        var wait = delay ?? timings.TurnDuration;
        if (wait < TimeSpan.Zero) wait = TimeSpan.Zero;

        scheduler.Schedule(TurnTimerKey(gameId), wait, async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var scopedGameService = scope.ServiceProvider.GetRequiredService<IBatalhaRuralGameService>();

            var result = await scopedGameService.TimeoutTurnAsync(gameId);

            if (result == null) return;

            if (result.GameEnded && result.WinnerPlayerNum != null)
            {
                await hubContext.Clients.Group(code).SendAsync("ChatMessageReceived", SystemMessage(
                    $"Partida encerrada: um jogador deixou o tempo acabar {timings.MaxConsecutiveTimeouts} vezes seguidas."));
                await hubContext.Clients.Group(code).SendAsync("GameEnded", new GameEndedDto
                {
                    WinnerPlayerNum = result.WinnerPlayerNum.Value,
                    Reason = "Inactivity"
                });
                return;
            }

            await hubContext.Clients.Group(code).SendAsync("TurnTimedOut", result);
            ScheduleTurnTimer(gameId, code);
        });
    }

    private void ScheduleAbandonTimer(string gameId, string code, string disconnectedUserId)
    {
        scheduler.Schedule(AbandonTimerKey(gameId, disconnectedUserId), timings.ReconnectGrace, async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var scopedGameService = scope.ServiceProvider.GetRequiredService<IBatalhaRuralGameService>();

            var result = await scopedGameService.ForfeitAsync(gameId, disconnectedUserId);

            if (result?.WinnerPlayerNum == null) return;

            CancelGameTimers(gameId);
            await hubContext.Clients.Group(code).SendAsync("GameEnded", new GameEndedDto
            {
                WinnerPlayerNum = result.WinnerPlayerNum.Value,
                Reason = "Abandonment"
            });
        });
    }

    // Once nobody in a room with a match is connected anymore, give them the reconnect window
    // and then drop the room, so finished/abandoned matches don't keep rooms in memory forever.
    private void ScheduleRoomCleanup(string code)
    {
        scheduler.Schedule(RoomCleanupKey(code), timings.RoomCleanupDelay, () =>
        {
            roomRegistry.RemoveRoomIfAbandoned(code);
            return Task.CompletedTask;
        });
    }

    private void CancelGameTimers(string gameId)
    {
        scheduler.Cancel(TurnTimerKey(gameId));
        scheduler.Cancel(PlacementTimerKey(gameId));
    }

    private static bool HasConnectedPlayers(GameRoom room) =>
        room.Player1?.Connected == true || room.Player2?.Connected == true;

    // Hub calls carry the room code from the client, which can't be trusted - resolve the
    // caller's actual room from their connection and reject any mismatch.
    private GameRoom RequireCallerRoom(string code)
    {
        var room = roomRegistry.GetRoomByConnection(Context.ConnectionId);

        if (room == null || !string.Equals(room.Code, code, StringComparison.OrdinalIgnoreCase))
            throw new HubException("Você não está nesta sala");

        return room;
    }

    private GameRoom RequireCallerGame(string code, string gameId)
    {
        var room = RequireCallerRoom(code);

        if (room.GameId != gameId) throw new HubException("Partida não encontrada nesta sala");

        return room;
    }

    private static string TurnTimerKey(string gameId) => $"turn:{gameId}";

    private static string PlacementTimerKey(string gameId) => $"placement:{gameId}";

    private static string AbandonTimerKey(string gameId, string userId) => $"abandon:{gameId}:{userId}";

    private static string RoomCleanupKey(string code) => $"room-cleanup:{code}";

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
            Player1 = ToPlayerDto(room.Player1),
            Player2 = ToPlayerDto(room.Player2)
        };
    }

    private static RoomPlayerDto? ToPlayerDto(RoomPlayer? player)
    {
        return player == null
            ? null
            : new RoomPlayerDto
            {
                UserId = player.UserId,
                Nickname = player.Nickname,
                Ready = player.Ready,
                Connected = player.Connected,
                WantsRematch = player.WantsRematch
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
