namespace gameHubBack.DTOs.Rooms;

public class RoomStateDto
{
    public required string Code { get; set; }
    public string? GameId { get; set; }
    public RoomPlayerDto? Player1 { get; set; }
    public RoomPlayerDto? Player2 { get; set; }
}
