using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Entities.BatalhaRural;

public class GameToken
{
    public required TokenType Type { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public Direction Direction { get; set; }
}
