using gameHubBack.Data;
using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace gameHubBack.Services.BatalhaRural;

public class GameService(AppDbContext context, GameLocks locks) : IBatalhaRuralGameService
{
    private const int BoardSize = 10;
    public static readonly TimeSpan TurnDuration = TimeSpan.FromSeconds(30);

    public async Task<BatalhaRuralGame> CreateGameAsync(string userId1, string nickname1, string userId2, string nickname2)
    {
        var game = new BatalhaRuralGame();

        game.Players.Add(CreatePlayer(game.Id, userId1, nickname1, PlayerNum.Player1));
        game.Players.Add(CreatePlayer(game.Id, userId2, nickname2, PlayerNum.Player2));

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

    public Task<bool> SetPlayerReadyAsync(string gameId, string userId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);
        var player = game?.Players.SingleOrDefault(p => p.UserId == userId);

        if (player == null) return false;

        player.PlayerStatus = PlayerStatus.Ready;

        var bothReady = game!.Players.All(p => p.PlayerStatus == PlayerStatus.Ready);

        if (bothReady)
        {
            game.Status = GameStatus.InProgress;
            game.CurrentTurnPlayerNum = Random.Shared.Next(2) == 0 ? PlayerNum.Player1 : PlayerNum.Player2;
            game.TurnExpiresAt = DateTime.UtcNow.Add(TurnDuration);
        }

        await context.SaveChangesAsync();

        return bothReady;
    });

    public Task<AttackResult?> AttackAsync(string gameId, string attackerUserId, int x, int y) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);
        var attacker = game?.Players.SingleOrDefault(p => p.UserId == attackerUserId);

        if (game == null || attacker == null) return null;

        if (game.CurrentTurnPlayerNum != attacker.PlayerNum) return AttackResult.Fail(AttackFailureReason.NotYourTurn);

        var defender = game.Players.Single(p => p.PlayerNum != attacker.PlayerNum);

        if (x < 0 || x >= defender.BoardTiles.Length || y < 0 || y >= defender.BoardTiles.Length)
            return AttackResult.Fail(AttackFailureReason.OutOfBounds);

        if (defender.ShotsReceived.Any(s => s.X == x && s.Y == y))
            return AttackResult.Fail(AttackFailureReason.AlreadyAttacked);

        var hit = defender.BoardTiles[y][x] != "O";
        defender.ShotsReceived.Add(new Shot { X = x, Y = y, Hit = hit });

        var totalPieceCells = defender.BoardTiles.SelectMany(row => row).Count(c => c != "O");
        var hitCells = defender.ShotsReceived.Count(s => s.Hit);
        var defenderDefeated = hitCells >= totalPieceCells;

        AttackResult result;

        if (defenderDefeated)
        {
            attacker.PlayerStatus = PlayerStatus.Winner;
            defender.PlayerStatus = PlayerStatus.Loser;
            game.Status = GameStatus.Ended;
            game.CurrentTurnPlayerNum = null;
            game.TurnExpiresAt = null;

            result = new AttackResult
            {
                Success = true,
                Hit = hit,
                X = x,
                Y = y,
                AttackerPlayerNum = attacker.PlayerNum,
                DefenderPlayerNum = defender.PlayerNum,
                GameEnded = true,
                WinnerPlayerNum = attacker.PlayerNum
            };
        }
        else
        {
            if (!hit) game.CurrentTurnPlayerNum = defender.PlayerNum;
            game.TurnExpiresAt = DateTime.UtcNow.Add(TurnDuration);

            result = new AttackResult
            {
                Success = true,
                Hit = hit,
                X = x,
                Y = y,
                AttackerPlayerNum = attacker.PlayerNum,
                DefenderPlayerNum = defender.PlayerNum,
                GameEnded = false,
                NextTurnPlayerNum = game.CurrentTurnPlayerNum
            };
        }

        await context.SaveChangesAsync();

        return result;
    });

    public Task<AttackResult?> TimeoutTurnAsync(string gameId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);

        if (game == null || game.Status != GameStatus.InProgress || game.CurrentTurnPlayerNum == null) return null;

        var previous = game.CurrentTurnPlayerNum.Value;
        var next = previous == PlayerNum.Player1 ? PlayerNum.Player2 : PlayerNum.Player1;

        game.CurrentTurnPlayerNum = next;
        game.TurnExpiresAt = DateTime.UtcNow.Add(TurnDuration);

        await context.SaveChangesAsync();

        return new AttackResult
        {
            Success = true,
            AttackerPlayerNum = previous,
            DefenderPlayerNum = next,
            GameEnded = false,
            NextTurnPlayerNum = next
        };
    });

    public Task<DateTime?> RefreshTurnDeadlineAsync(string gameId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);

        if (game == null || game.Status != GameStatus.InProgress || game.CurrentTurnPlayerNum == null) return (DateTime?)null;

        game.TurnExpiresAt = DateTime.UtcNow.Add(TurnDuration);
        await context.SaveChangesAsync();

        return game.TurnExpiresAt;
    });

    public Task<AttackResult?> ForfeitAsync(string gameId, string disconnectedUserId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);
        var loser = game?.Players.SingleOrDefault(p => p.UserId == disconnectedUserId);

        if (game == null || loser == null || game.Status == GameStatus.Ended) return null;

        var winner = game.Players.Single(p => p.UserId != disconnectedUserId);

        winner.PlayerStatus = PlayerStatus.Winner;
        loser.PlayerStatus = PlayerStatus.Loser;
        game.Status = GameStatus.Ended;
        game.CurrentTurnPlayerNum = null;
        game.TurnExpiresAt = null;

        await context.SaveChangesAsync();

        return new AttackResult
        {
            Success = true,
            AttackerPlayerNum = winner.PlayerNum,
            DefenderPlayerNum = loser.PlayerNum,
            GameEnded = true,
            WinnerPlayerNum = winner.PlayerNum
        };
    });

    private static BatalhaRuralPlayer CreatePlayer(string gameId, string userId, string nickname, PlayerNum playerNum)
    {
        return new BatalhaRuralPlayer
        {
            GameId = gameId,
            UserId = userId,
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
