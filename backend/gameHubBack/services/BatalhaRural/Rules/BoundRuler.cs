namespace gameHubBack.Services.BatalhaRural.Rules;

public static class BoundRuler
{
    public static bool IsOutOfBounds(TokenInfo tokenInfo, string[][] tiles)
    {
        if (tokenInfo.BaseZeroFinalPosX >= tiles.Length || tokenInfo.BaseZeroFinalPosY >= tiles.Length) return true;

        if (tokenInfo.BaseZeroFinalPosX < 0 || tokenInfo.BaseZeroFinalPosY < 0) return true;

        return false;
    }
}