using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.Extensions;

public static class BatalhaRuralExtensions
{
    public static GameStateDto ToDto(this BatalhaRuralGame game)
    {
        return new GameStateDto
        {
            Id = game.Id,
            Status = game.Status,
            Players = game.Players.Select(p => p.ToDto()).ToList()
        };
    }

    public static PlayerStateDto ToDto(this BatalhaRuralPlayer player)
    {
        return new PlayerStateDto
        {
            Id = player.Id,
            Nickname = player.Nickname,
            PlayerNum = player.PlayerNum,
            PlayerStatus = player.PlayerStatus,
            Board = player.BoardTiles,
            Tokens = player.Tokens.Select((token, index) => token.ToDto(index)).ToList()
        };
    }

    public static TokenDto ToDto(this GameToken token, int index)
    {
        return new TokenDto
        {
            Index = index,
            Type = token.Type,
            Name = TokenCatalog.GetName(token.Type),
            Character = TokenCatalog.GetCharacter(token.Type),
            Size = TokenCatalog.GetSize(token.Type),
            PositionX = token.PositionX,
            PositionY = token.PositionY,
            Direction = token.Direction
        };
    }
}
