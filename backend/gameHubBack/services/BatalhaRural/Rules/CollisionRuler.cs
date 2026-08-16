using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Services.BatalhaRural.Rules;

public static class CollisionRuler
{
    public static bool IsTileOccupied(TokenInfo tokenInfo, string[][] tiles)
    {
        var tileStatus = tiles[tokenInfo.BaseZeroPosY][tokenInfo.BaseZeroPosX];

        return tileStatus != "O";
    }

    public static bool IsTokenHorizontallyOverlapping(TokenInfo tokenInfo, string[][] tiles, Direction direction)
    {
        if (direction == Direction.Right)
        {
            if (IsTokenDiagonal(tokenInfo, Axis.X)) return true;

            for (var i = tokenInfo.BaseZeroPosX; i < tokenInfo.BaseZeroPosX + tokenInfo.TokenSize; i++)
            {
                if (tiles[tokenInfo.BaseZeroPosY][i] != "O") return true;
            }
        }

        if (direction == Direction.Left)
        {
            if (IsTokenDiagonal(tokenInfo, Axis.X)) return false;

            for (var i = 0; i < tokenInfo.TokenSize; i++)
            {
                if (tiles[tokenInfo.BaseZeroPosY][tokenInfo.BaseZeroPosX - i] != "O") return true;
            }
        }

        return false;
    }

    public static bool IsTokenVerticallyOverlapping(TokenInfo tokenInfo, string[][] tiles, Direction direction)
    {
        if (direction == Direction.Up)
        {
            if (IsTokenDiagonal(tokenInfo, Axis.Y)) return false;

            for (var i = 0; i < tokenInfo.TokenSize; i++)
            {
                if (tiles[tokenInfo.BaseZeroPosY - i][tokenInfo.BaseZeroPosX] != "O") return true;
            }
        }

        if (direction == Direction.Down)
        {
            if (IsTokenDiagonal(tokenInfo, Axis.Y)) return false;

            for (var i = tokenInfo.BaseZeroPosY; i < tokenInfo.BaseZeroPosY + tokenInfo.TokenSize; i++)
            {
                if (tiles[i][tokenInfo.BaseZeroPosX] != "O") return true;
            }
        }

        return false;
    }

    private static bool IsTokenDiagonal(TokenInfo tokenInfo, Axis axis)
    {
        return axis switch
        {
            Axis.X => tokenInfo.BaseZeroFinalPosY != tokenInfo.BaseZeroPosY,
            Axis.Y => tokenInfo.BaseZeroFinalPosX != tokenInfo.BaseZeroPosX,
            _ => false
        };
    }
}