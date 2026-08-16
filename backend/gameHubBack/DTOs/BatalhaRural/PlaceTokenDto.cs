using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class PlaceTokenDto
{
    public required int PositionX { get; set; }
    public required int PositionY { get; set; }
    public required Direction Direction { get; set; }
}
