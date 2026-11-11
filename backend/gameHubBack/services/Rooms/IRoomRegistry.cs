namespace gameHubBack.Services.Rooms;

public interface IRoomRegistry
{
    GameRoom CreateRoom(string userId, string nickname, string connectionId);

    GameRoom? TryJoinRoom(string code, string userId, string nickname, string connectionId);

    IReadOnlyCollection<GameRoom> GetOpenRooms();

    GameRoom? GetRoom(string code);

    GameRoom? GetRoomByConnection(string connectionId);

    bool SetReady(string code, string userId, bool ready);

    void SetGameId(string code, string gameId);

    GameRoom? ResetRoom(string code);

    bool RemoveRoomIfAbandoned(string code);

    bool RequestRematch(string code, string userId);

    GameRoom? TryRejoin(string code, string userId, string connectionId);

    GameRoom? HandleDisconnect(string connectionId);
}
