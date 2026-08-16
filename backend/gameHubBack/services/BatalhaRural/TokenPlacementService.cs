namespace gameHubBack.Services.BatalhaRural;

public static class TokenPlacementService
{
    public static void PlaceTokenUp(TokenInfo tokenInfo, string[][] tiles)
    {
        for (var k = 0; k < tokenInfo.TokenSize; k++)
            tiles[tokenInfo.BaseZeroPosY - k][tokenInfo.BaseZeroPosX] = tokenInfo.TokenChar;
    }

    public static void PlaceTokenRight(TokenInfo tokenInfo, string[][] tiles)
    {
        for (var k = 0; k < tokenInfo.TokenSize; k++)
            tiles[tokenInfo.BaseZeroPosY][tokenInfo.BaseZeroPosX + k] = tokenInfo.TokenChar;
    }

    public static void PlaceTokenDown(TokenInfo tokenInfo, string[][] tiles)
    {
        for (var k = 0; k < tokenInfo.TokenSize; k++)
            tiles[tokenInfo.BaseZeroPosY + k][tokenInfo.BaseZeroPosX] = tokenInfo.TokenChar;
    }

    public static void PlaceTokenLeft(TokenInfo tokenInfo, string[][] tiles)
    {
        for (var k = 0; k < tokenInfo.TokenSize; k++)
            tiles[tokenInfo.BaseZeroPosY][tokenInfo.BaseZeroPosX - k] = tokenInfo.TokenChar;
    }
}