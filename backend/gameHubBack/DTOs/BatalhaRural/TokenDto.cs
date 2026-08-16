using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class TokenDto
{
    public required int Index { get; set; }
    public required TokenType Type { get; set; }
    public required string Name { get; set; }
    public required string Character { get; set; }
    public required int Size { get; set; }
    public required int PositionX { get; set; }
    public required int PositionY { get; set; }
    public required Direction Direction { get; set; }
}
