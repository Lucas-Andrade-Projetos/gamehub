using System.Collections.Concurrent;

namespace gameHubBack.Services.Rooms;

public class RoomRegistry : IRoomRegistry
{
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly ConcurrentDictionary<string, string> _connectionToRoom = new();

    public GameRoom CreateRoom(string userId, string nickname, string connectionId)
    {
        GameRoom room;
        string code;

        do
        {
            code = GenerateCode();
            room = new GameRoom
            {
                Code = code,
                Player1 = new RoomPlayer { UserId = userId, Nickname = nickname, ConnectionId = connectionId }
            };
        } while (!_rooms.TryAdd(code, room));

        _connectionToRoom[connectionId] = code;

        return room;
    }

    public GameRoom? TryJoinRoom(string code, string userId, string nickname, string connectionId)
    {
        if (!_rooms.TryGetValue(code, out var room)) return null;
        if (room.Player1 != null && room.Player2 != null) return null;

        var player = new RoomPlayer { UserId = userId, Nickname = nickname, ConnectionId = connectionId };

        if (room.Player1 == null) room.Player1 = player;
        else room.Player2 = player;

        _connectionToRoom[connectionId] = code;

        return room;
    }

    public IReadOnlyCollection<GameRoom> GetOpenRooms()
    {
        return _rooms.Values.Where(r => r.GameId == null && (r.Player1 == null || r.Player2 == null)).ToList();
    }

    public GameRoom? GetRoom(string code)
    {
        _rooms.TryGetValue(code, out var room);
        return room;
    }

    public bool SetReady(string code, string userId, bool ready)
    {
        var room = GetRoom(code);
        if (room == null) return false;

        if (room.Player1?.UserId == userId) room.Player1.Ready = ready;
        else if (room.Player2?.UserId == userId) room.Player2.Ready = ready;
        else return false;

        return room.Player1?.Ready == true && room.Player2?.Ready == true;
    }

    public void SetGameId(string code, string gameId)
    {
        if (_rooms.TryGetValue(code, out var room)) room.GameId = gameId;
    }

    public GameRoom? ResetRoom(string code)
    {
        if (!_rooms.TryGetValue(code, out var room)) return null;

        room.GameId = null;
        if (room.Player1 != null) room.Player1.Ready = false;
        if (room.Player2 != null) room.Player2.Ready = false;

        return room;
    }

    public GameRoom? RemoveConnection(string connectionId)
    {
        if (!_connectionToRoom.TryRemove(connectionId, out var code)) return null;
        if (!_rooms.TryGetValue(code, out var room)) return null;

        if (room.Player1?.ConnectionId == connectionId) room.Player1 = null;
        else if (room.Player2?.ConnectionId == connectionId) room.Player2 = null;

        if (room.Player1 == null && room.Player2 == null)
        {
            _rooms.TryRemove(code, out _);
            return null;
        }

        return room;
    }

    private static string GenerateCode()
    {
        var random = Random.Shared;
        return new string(Enumerable.Range(0, 5).Select(_ => CodeChars[random.Next(CodeChars.Length)]).ToArray());
    }
}
