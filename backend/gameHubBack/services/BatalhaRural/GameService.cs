using gameHubBack.Data;
using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace gameHubBack.Services.BatalhaRural;

public class GameService(AppDbContext context) : IBatalhaRuralGameService
{
    private const int BoardSize = 10;

    public async Task<BatalhaRuralGame> CreateGameAsync(string player1Nickname, string player2Nickname)
    {
        var game = new BatalhaRuralGame();

        game.Players.Add(CreatePlayer(game.Id, player1Nickname, PlayerNum.Player1));
        game.Players.Add(CreatePlayer(game.Id, player2Nickname, PlayerNum.Player2));

        context.BatalhaRuralGames.Add(game);
        await context.SaveChangesAsync();

        return game;
    }

    public async Task<BatalhaRuralGame?> GetGameAsync(string gameId)
    {
        return await context.BatalhaRuralGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<PlacementResult?> PlaceTokenAsync(BatalhaRuralPlayer player, int tokenIndex, int positionX, int positionY, Direction direction)
    {
        if (tokenIndex < 0 || tokenIndex >= player.Tokens.Count) return null;

        var candidate = new GameToken
        {
            Type = player.Tokens[tokenIndex].Type,
            PositionX = positionX,
            PositionY = positionY,
            Direction = direction
        };

        var result = PlacementHelper.TryPlaceToken(candidate, player.BoardTiles);

        if (result.Success)
        {
            player.Tokens[tokenIndex] = candidate;
            await context.SaveChangesAsync();
        }

        return result;
    }

    private static BatalhaRuralPlayer CreatePlayer(string gameId, string nickname, PlayerNum playerNum)
    {
        return new BatalhaRuralPlayer
        {
            GameId = gameId,
            Nickname = nickname,
            PlayerNum = playerNum,
            BoardTiles = CreateEmptyBoard(BoardSize),
            Tokens = TokenCatalog.CreateDefaultTokens()
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
