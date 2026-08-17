namespace gameHubBack.Services.Rooms;

public class RoomPlayer
{
    public required string UserId { get; set; }
    public required string Nickname { get; set; }
    public required string ConnectionId { get; set; }
    public bool Ready { get; set; }
    public bool Connected { get; set; } = true;
}
