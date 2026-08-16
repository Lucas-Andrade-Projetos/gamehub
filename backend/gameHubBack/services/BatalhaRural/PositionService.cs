using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural.Rules;

namespace gameHubBack.Services.BatalhaRural;

public static class PositionService
{
    public static bool CanPlaceToken(TokenInfo tokenInfo, string[][] tiles)
    {
        if (BoundRuler.IsOutOfBounds(tokenInfo, tiles) || CollisionRuler.IsTileOccupied(tokenInfo, tiles)) return false;

        return true;
    }

    public static bool IsTokenPathOccupied(TokenInfo tokenInfo, string[][] tiles, Axis axis, Direction direction)
    {
        return axis switch
        {
            Axis.X => CollisionRuler.IsTokenHorizontallyOverlapping(tokenInfo, tiles, direction),
            Axis.Y => CollisionRuler.IsTokenVerticallyOverlapping(tokenInfo, tiles, direction),
            _ => true
        };
    }
}