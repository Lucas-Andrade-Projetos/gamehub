using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Services.BatalhaRural;

public class TokenInfo
{
    public int BaseOnePosX { get; }
    public int BaseOnePosY { get; }
    public int TokenSize { get; }
    public Direction TokenDir { get; }
    public string TokenName { get; }
    public string TokenChar { get; }

    public int BaseOneFinalPosX => CoordinateHelper.CalcHorizontalFinalPosition(BaseOnePosX, TokenSize, TokenDir);
    public int BaseOneFinalPosY => CoordinateHelper.CalcVerticalFinalPosition(BaseOnePosY, TokenSize, TokenDir);
    public int BaseZeroFinalPosX => BaseOneFinalPosX - 1;
    public int BaseZeroFinalPosY => BaseOneFinalPosY - 1;
    public int BaseZeroPosX => BaseOnePosX - 1;
    public int BaseZeroPosY => BaseOnePosY - 1;

    public Axis TokenAxis => TokenDir is Direction.Up or Direction.Down ? Axis.Y : Axis.X;

    public TokenInfo(GameToken token)
    {
        TokenSize = TokenCatalog.GetSize(token.Type);
        TokenDir = token.Direction;
        TokenName = TokenCatalog.GetName(token.Type);
        TokenChar = TokenCatalog.GetCharacter(token.Type);
        BaseOnePosX = token.PositionX;
        BaseOnePosY = token.PositionY;
    }
}