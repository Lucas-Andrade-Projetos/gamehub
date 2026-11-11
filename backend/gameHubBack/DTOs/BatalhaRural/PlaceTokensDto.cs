using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class PlaceTokensDto
{
    public required List<TokenPlacementDto> Tokens { get; set; }
}

public class TokenPlacementDto
{
    public required int Index { get; set; }
    public required int PositionX { get; set; }
    public required int PositionY { get; set; }
    public required Direction Direction { get; set; }
}
