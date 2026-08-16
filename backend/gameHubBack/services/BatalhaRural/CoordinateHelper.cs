using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Services.BatalhaRural;

public static class CoordinateHelper
{
    public static int CalcHorizontalFinalPosition(int baseOnePosX, int size, Direction direction)
    {
        if (direction == Direction.Right) return baseOnePosX + size - 1;
        if (direction == Direction.Left) return baseOnePosX - size + 1;
        return baseOnePosX;
    }

    public static int CalcVerticalFinalPosition(int baseOnePosY, int size, Direction direction)
    {
        if (direction == Direction.Down) return baseOnePosY + size - 1;
        if (direction == Direction.Up) return baseOnePosY - size + 1;
        return baseOnePosY;
    }
}