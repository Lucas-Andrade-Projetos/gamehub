namespace gameHubBack.Services.Rooms;

public class GameRoom
{
    public required string Code { get; set; }
    public RoomPlayer? Player1 { get; set; }
    public RoomPlayer? Player2 { get; set; }
    public string? GameId { get; set; }
}
