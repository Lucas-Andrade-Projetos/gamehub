using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class GameStateDto
{
    public required string Id { get; set; }
    public required GameStatus Status { get; set; }
    public required PlayerNum ViewerPlayerNum { get; set; }
    public PlayerNum? CurrentTurnPlayerNum { get; set; }
    public DateTime? TurnExpiresAt { get; set; }
    public required List<PlayerStateDto> Players { get; set; }
}
