namespace gameHubBack.Services.Rooms;

public interface IRoomRegistry
{
    GameRoom CreateRoom(string userId, string nickname, string connectionId);

    GameRoom? TryJoinRoom(string code, string userId, string nickname, string connectionId);

    IReadOnlyCollection<GameRoom> GetOpenRooms();

    GameRoom? GetRoom(string code);

    bool SetReady(string code, string userId, bool ready);

    void SetGameId(string code, string gameId);

    GameRoom? ResetRoom(string code);

    GameRoom? TryRejoin(string code, string userId, string connectionId);

    GameRoom? HandleDisconnect(string connectionId);
}
