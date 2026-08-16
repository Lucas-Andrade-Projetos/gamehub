namespace gameHubBack.Services.BatalhaRural.Validators;

public static class InputValidator
{
    public static bool IsInputValid(TokenInfo tokenInfo, string[][] tiles)
    {
        var boardLength = tiles.Length;

        if (tokenInfo.BaseOnePosX > boardLength || tokenInfo.BaseOnePosY > boardLength ||
            tokenInfo.BaseOnePosX < 1 || tokenInfo.BaseOnePosY < 1)
        {
            return false;
        }

        return true;
    }
}