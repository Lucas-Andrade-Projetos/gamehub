namespace gameHubBack.Services.BatalhaRural;

public static class BoardFactory
{
    public const int DefaultSize = 10;
    public const string EmptyTile = "O";

    public static string[][] CreateEmpty(int size = DefaultSize)
    {
        var tiles = new string[size][];

        for (var i = 0; i < size; i++)
        {
            tiles[i] = new string[size];
            for (var j = 0; j < size; j++) tiles[i][j] = EmptyTile;
        }

        return tiles;
    }

    public static int CountOccupied(string[][] tiles) => tiles.SelectMany(row => row).Count(c => c != EmptyTile);
}
