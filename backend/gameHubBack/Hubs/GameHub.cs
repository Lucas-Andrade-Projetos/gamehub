using System.Security.Claims;
using gameHubBack.DTOs.Rooms;
using gameHubBack.Interfaces;
using gameHubBack.Services.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace gameHubBack.Hubs;

[Authorize]
public class GameHub(IRoomRegistry roomRegistry, IBatalhaRuralGameService gameService) : Hub
{
    private const string RoomBrowserGroup = "room-browser";

    public async Task<string> CreateRoom()
    {
        var (userId, nickname) = GetIdentity();

        var room = roomRegistry.CreateRoom(userId, nickname, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
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

        if (bothReady) await Clients.Group(code).SendAsync("BattleStarting");
    }

    public async Task Attack(string gameId, string code, int x, int y)
    {
        var (userId, _) = GetIdentity();

        var result = await gameService.AttackAsync(gameId, userId, x, y);

        if (result == null) return;

        await Clients.Group(code).SendAsync("AttackResolved", result);

        if (result.GameEnded) await Clients.Group(code).SendAsync("GameEnded", result.WinnerPlayerNum);
    }

    public async Task ReturnToRoom(string code)
    {
        var room = roomRegistry.ResetRoom(code);

        if (room != null) await Clients.Group(code).SendAsync("RoomUpdated", ToStateDto(room));
    }

    public async Task LeaveRoom(string code)
    {
        var (_, nickname) = GetIdentity();

        var room = roomRegistry.RemoveConnection(Context.ConnectionId);
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
        var room = roomRegistry.RemoveConnection(Context.ConnectionId);

        if (room != null)
        {
            await Clients.Group(room.Code).SendAsync("RoomUpdated", ToStateDto(room));

            if (nickname != null)
            {
                await Clients.Group(room.Code).SendAsync("ChatMessageReceived", SystemMessage($"{nickname} saiu da sala"));
            }
        }

        await BroadcastOpenRooms();
        await base.OnDisconnectedAsync(exception);
    }

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
            Player1 = room.Player1 == null ? null : new RoomPlayerDto { Nickname = room.Player1.Nickname, Ready = room.Player1.Ready },
            Player2 = room.Player2 == null ? null : new RoomPlayerDto { Nickname = room.Player2.Nickname, Ready = room.Player2.Ready }
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
