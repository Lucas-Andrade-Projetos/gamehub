using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Entities.BatalhaRural;

public class BatalhaRuralGame
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public GameStatus Status { get; set; } = GameStatus.Preparing;
    public PlayerNum? CurrentTurnPlayerNum { get; set; }
    public DateTime? TurnExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<BatalhaRuralPlayer> Players { get; set; } = [];
}
