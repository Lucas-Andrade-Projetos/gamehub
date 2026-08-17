namespace gameHubBack.DTOs.Rooms;

public class RoomPlayerDto
{
    public required string UserId { get; set; }
    public required string Nickname { get; set; }
    public required bool Ready { get; set; }
    public required bool Connected { get; set; }
}
