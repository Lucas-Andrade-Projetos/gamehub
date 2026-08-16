using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural.Rules;
using gameHubBack.Services.BatalhaRural.Validators;

namespace gameHubBack.Services.BatalhaRural;

public static class PlacementHelper
{
    public static PlacementResult TryPlaceToken(GameToken token, string[][] tiles)
    {
        var tokenInfo = new TokenInfo(token);

        if (!InputValidator.IsInputValid(tokenInfo, tiles))
            return PlacementResult.Fail(PlacementFailureReason.InvalidInput);

        if (BoundRuler.IsOutOfBounds(tokenInfo, tiles))
            return PlacementResult.Fail(PlacementFailureReason.OutOfBounds);

        if (CollisionRuler.IsTileOccupied(tokenInfo, tiles))
            return PlacementResult.Fail(PlacementFailureReason.Collision);

        if (tokenInfo.TokenSize > 1)
        {
            if (PositionService.IsTokenPathOccupied(tokenInfo, tiles, tokenInfo.TokenAxis, tokenInfo.TokenDir))
                return PlacementResult.Fail(PlacementFailureReason.Collision);

            switch (tokenInfo.TokenDir)
            {
                case Direction.Down:
                    TokenPlacementService.PlaceTokenDown(tokenInfo, tiles);
                    break;
                case Direction.Up:
                    TokenPlacementService.PlaceTokenUp(tokenInfo, tiles);
                    break;
                case Direction.Right:
                    TokenPlacementService.PlaceTokenRight(tokenInfo, tiles);
                    break;
                case Direction.Left:
                    TokenPlacementService.PlaceTokenLeft(tokenInfo, tiles);
                    break;
            }
        }
        else
        {
            tiles[tokenInfo.BaseZeroPosY][tokenInfo.BaseZeroPosX] = tokenInfo.TokenChar;
        }

        return PlacementResult.Ok();
    }
}