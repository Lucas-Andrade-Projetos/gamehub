using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class GameStateDto
{
    public required string Id { get; set; }
    public required GameStatus Status { get; set; }
    public required List<PlayerStateDto> Players { get; set; }
}
