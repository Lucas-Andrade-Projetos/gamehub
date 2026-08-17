using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.Extensions;

public static class BatalhaRuralExtensions
{
    public static GameStateDto ToDto(this BatalhaRuralGame game, string viewerUserId)
    {
        var viewer = game.Players.First(p => p.UserId == viewerUserId);

        return new GameStateDto
        {
            Id = game.Id,
            Status = game.Status,
            ViewerPlayerNum = viewer.PlayerNum,
            CurrentTurnPlayerNum = game.CurrentTurnPlayerNum,
            TurnExpiresAt = game.TurnExpiresAt,
            Players = game.Players.Select(p => p.ToDto(viewerUserId)).ToList()
        };
    }

    public static PlayerStateDto ToDto(this BatalhaRuralPlayer player, string viewerUserId)
    {
        var isOwner = player.UserId == viewerUserId;

        return new PlayerStateDto
        {
            Id = player.Id,
            Nickname = player.Nickname,
            PlayerNum = player.PlayerNum,
            PlayerStatus = player.PlayerStatus,
            Board = isOwner ? player.BoardTiles : CreateEmptyBoard(player.BoardTiles.Length),
            Tokens = isOwner ? player.Tokens.Select((token, index) => token.ToDto(index)).ToList() : [],
            ShotsReceived = player.ShotsReceived.Select(s => new ShotDto { X = s.X, Y = s.Y, Hit = s.Hit }).ToList()
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

    private static string[][] CreateEmptyBoard(int size)
    {
        var tiles = new string[size][];

        for (var i = 0; i < size; i++)
        {
            tiles[i] = new string[size];
            for (var j = 0; j < size; j++) tiles[i][j] = "O";
        }

        return tiles;
    }
}
