using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Entities.BatalhaRural;

public class BatalhaRuralPlayer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string GameId { get; set; }
    public BatalhaRuralGame? Game { get; set; }
    public required string UserId { get; set; }
    public required string Nickname { get; set; }
    public required PlayerNum PlayerNum { get; set; }
    public PlayerStatus PlayerStatus { get; set; } = PlayerStatus.Preparing;
    public required string[][] BoardTiles { get; set; }
    public required List<GameToken> Tokens { get; set; }
}
