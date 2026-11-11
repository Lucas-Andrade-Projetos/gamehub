using gameHubBack.Data;
using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace gameHubBack.Services.BatalhaRural;

// While a game is Preparing, TurnExpiresAt holds the placement deadline; once InProgress it
// holds the current turn's deadline.
public class GameService(AppDbContext context, GameLocks locks, BatalhaRuralTimings timings) : IBatalhaRuralGameService
{
    private const int BoardSize = BoardFactory.DefaultSize;

    public async Task<BatalhaRuralGame> CreateGameAsync(string userId1, string nickname1, string userId2, string nickname2)
    {
        var game = new BatalhaRuralGame { TurnExpiresAt = DateTime.UtcNow.Add(timings.PlacementDuration) };

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

    // Places the player's whole set at once on a fresh board, so a retry (or a changed layout)
    // never collides with cells left over from a previous attempt. All-or-nothing: on any
    // failure the stored board/tokens stay untouched.
    public Task<PlacementResult?> PlaceAllTokensAsync(string gameId, string userId, IReadOnlyList<TokenPlacement> placements) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);
        var player = game?.Players.SingleOrDefault(p => p.UserId == userId);

        if (game == null || player == null) return null;

        if (game.Status != GameStatus.Preparing || player.PlayerStatus != PlayerStatus.Preparing)
            return PlacementResult.Fail(PlacementFailureReason.NotAllowed);

        var indices = placements.Select(p => p.Index).ToList();

        if (indices.Count != player.Tokens.Count
            || indices.Distinct().Count() != indices.Count
            || indices.Any(i => i < 0 || i >= player.Tokens.Count))
            return PlacementResult.Fail(PlacementFailureReason.IncompleteSet);

        var board = BoardFactory.CreateEmpty(BoardSize);
        var tokens = new List<GameToken>(player.Tokens);

        foreach (var placement in placements)
        {
            var type = player.Tokens[placement.Index].Type;

            if (!Enum.IsDefined(placement.Direction)
                || (TokenCatalog.GetSize(type) > 1 && placement.Direction == Direction.Centered))
                return PlacementResult.Fail(PlacementFailureReason.InvalidInput, placement.Index);

            var candidate = new GameToken
            {
                Type = type,
                PositionX = placement.PositionX,
                PositionY = placement.PositionY,
                Direction = placement.Direction
            };

            var result = PlacementHelper.TryPlaceToken(candidate, board);

            if (!result.Success) return PlacementResult.Fail(result.Reason!.Value, placement.Index);

            tokens[placement.Index] = candidate;
        }

        player.BoardTiles = board;
        player.Tokens = tokens;
        await context.SaveChangesAsync();

        return PlacementResult.Ok();
    });

    public Task<ReadyResult> SetPlayerReadyAsync(string gameId, string userId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);
        var player = game?.Players.SingleOrDefault(p => p.UserId == userId);

        if (game == null || player == null || game.Status != GameStatus.Preparing) return ReadyResult.NotAllowed;

        if (player.PlayerStatus == PlayerStatus.Ready) return ReadyResult.WaitingOpponent;

        if (!IsFullyPlaced(player)) return ReadyResult.NotAllowed;

        player.PlayerStatus = PlayerStatus.Ready;

        var bothReady = game.Players.All(p => p.PlayerStatus == PlayerStatus.Ready);

        if (bothReady) StartBattle(game);

        await context.SaveChangesAsync();

        return bothReady ? ReadyResult.BothReady : ReadyResult.WaitingOpponent;
    });

    // Placement time ran out: whoever hasn't confirmed gets a random layout, and the battle
    // starts. Returns the nicknames that were auto-placed, or null if the game wasn't waiting
    // on placement anymore (already started, ended or cancelled).
    public Task<List<string>?> AutoCompletePlacementAsync(string gameId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);

        if (game == null || game.Status != GameStatus.Preparing) return null;

        var autoPlaced = new List<string>();

        foreach (var player in game.Players.Where(p => p.PlayerStatus != PlayerStatus.Ready))
        {
            var (board, tokens) = RandomPlacer.Place(player.Tokens, BoardSize, Random.Shared);

            player.BoardTiles = board;
            player.Tokens = tokens;
            player.PlayerStatus = PlayerStatus.Ready;
            autoPlaced.Add(player.Nickname);
        }

        StartBattle(game);
        await context.SaveChangesAsync();

        return autoPlaced;
    });

    public Task<AttackResult?> AttackAsync(string gameId, string attackerUserId, int x, int y) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);
        var attacker = game?.Players.SingleOrDefault(p => p.UserId == attackerUserId);

        if (game == null || attacker == null) return null;

        if (game.Status != GameStatus.InProgress || game.CurrentTurnPlayerNum != attacker.PlayerNum)
            return AttackResult.Fail(AttackFailureReason.NotYourTurn);

        var defender = game.Players.Single(p => p.PlayerNum != attacker.PlayerNum);

        if (x < 0 || x >= defender.BoardTiles.Length || y < 0 || y >= defender.BoardTiles.Length)
            return AttackResult.Fail(AttackFailureReason.OutOfBounds);

        if (defender.ShotsReceived.Any(s => s.X == x && s.Y == y))
            return AttackResult.Fail(AttackFailureReason.AlreadyAttacked);

        var hit = defender.BoardTiles[y][x] != BoardFactory.EmptyTile;
        defender.ShotsReceived.Add(new Shot { X = x, Y = y, Hit = hit });
        attacker.ConsecutiveTimeouts = 0;

        var totalPieceCells = BoardFactory.CountOccupied(defender.BoardTiles);
        var hitCells = defender.ShotsReceived.Count(s => s.Hit);
        var defenderDefeated = hitCells >= totalPieceCells;

        AttackResult result;

        if (defenderDefeated)
        {
            EndGame(game, winner: attacker, loser: defender);

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
            game.TurnExpiresAt = DateTime.UtcNow.Add(timings.TurnDuration);

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

        var idlePlayer = game.Players.Single(p => p.PlayerNum == previous);
        idlePlayer.ConsecutiveTimeouts++;

        // a player who keeps letting the clock run out loses, so a match where nobody plays
        // can't go on forever
        if (idlePlayer.ConsecutiveTimeouts >= timings.MaxConsecutiveTimeouts)
        {
            EndGame(game, winner: game.Players.Single(p => p.PlayerNum == next), loser: idlePlayer);

            await context.SaveChangesAsync();

            return new AttackResult
            {
                Success = true,
                AttackerPlayerNum = previous,
                DefenderPlayerNum = next,
                GameEnded = true,
                WinnerPlayerNum = next
            };
        }

        game.CurrentTurnPlayerNum = next;
        game.TurnExpiresAt = DateTime.UtcNow.Add(timings.TurnDuration);

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

    // The turn timer is paused while someone is disconnected. On reconnect:
    // - if it's the reconnecting player's turn, they keep whatever was left on their clock
    //   (plus a small grace to reload the page) - disconnecting doesn't buy a fresh turn;
    // - if it's the opponent's turn, the opponent gets a full turn, since their timer was
    //   paused through no fault of their own.
    public Task<DateTime?> RefreshTurnDeadlineAsync(string gameId, string rejoiningUserId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);

        if (game == null || game.Status != GameStatus.InProgress || game.CurrentTurnPlayerNum == null) return (DateTime?)null;

        var now = DateTime.UtcNow;
        var currentPlayer = game.Players.Single(p => p.PlayerNum == game.CurrentTurnPlayerNum);

        if (currentPlayer.UserId == rejoiningUserId)
        {
            var minimumDeadline = now.Add(timings.RejoinGrace);

            if (game.TurnExpiresAt == null || game.TurnExpiresAt < minimumDeadline) game.TurnExpiresAt = minimumDeadline;
        }
        else
        {
            game.TurnExpiresAt = now.Add(timings.TurnDuration);
        }

        await context.SaveChangesAsync();

        return game.TurnExpiresAt;
    });

    public Task<AttackResult?> ForfeitAsync(string gameId, string disconnectedUserId) => locks.RunAsync(gameId, async () =>
    {
        var game = await GetGameAsync(gameId);
        var loser = game?.Players.SingleOrDefault(p => p.UserId == disconnectedUserId);

        if (game == null || loser == null || game.Status is GameStatus.Ended or GameStatus.Cancelled) return null;

        var winner = game.Players.Single(p => p.UserId != disconnectedUserId);

        EndGame(game, winner, loser);

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

    // Rooms, turn timers and reconnect windows only live in memory, so any game still open
    // when the server starts can never be resumed - mark them as cancelled.
    public async Task<int> CancelUnfinishedGamesAsync()
    {
        return await context.BatalhaRuralGames
            .Where(g => g.Status == GameStatus.Preparing || g.Status == GameStatus.InProgress)
            .ExecuteUpdateAsync(s => s
                .SetProperty(g => g.Status, GameStatus.Cancelled)
                .SetProperty(g => g.CurrentTurnPlayerNum, (PlayerNum?)null)
                .SetProperty(g => g.TurnExpiresAt, (DateTime?)null));
    }

    private void StartBattle(BatalhaRuralGame game)
    {
        game.Status = GameStatus.InProgress;
        game.CurrentTurnPlayerNum = Random.Shared.Next(2) == 0 ? PlayerNum.Player1 : PlayerNum.Player2;
        game.TurnExpiresAt = DateTime.UtcNow.Add(timings.TurnDuration);
    }

    private static void EndGame(BatalhaRuralGame game, BatalhaRuralPlayer winner, BatalhaRuralPlayer loser)
    {
        winner.PlayerStatus = PlayerStatus.Winner;
        loser.PlayerStatus = PlayerStatus.Loser;
        game.Status = GameStatus.Ended;
        game.CurrentTurnPlayerNum = null;
        game.TurnExpiresAt = null;
    }

    private static bool IsFullyPlaced(BatalhaRuralPlayer player)
    {
        var expectedCells = player.Tokens.Sum(t => TokenCatalog.GetSize(t.Type));

        return BoardFactory.CountOccupied(player.BoardTiles) == expectedCells
            && player.Tokens.All(t => t.PositionX >= 1 && t.PositionY >= 1);
    }

    private static BatalhaRuralPlayer CreatePlayer(string gameId, string userId, string nickname, PlayerNum playerNum)
    {
        return new BatalhaRuralPlayer
        {
            GameId = gameId,
            UserId = userId,
            Nickname = nickname,
            PlayerNum = playerNum,
            BoardTiles = BoardFactory.CreateEmpty(BoardSize),
            Tokens = TokenCatalog.CreateDefaultTokens()
        };
    }
}
