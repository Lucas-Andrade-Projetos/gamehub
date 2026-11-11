using gameHubBack.Data;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static gameHubBack.Tests.GameServiceHarness;

namespace gameHubBack.Tests;

public class GameServicePlacementTests
{
    [Fact]
    public async Task New_game_is_preparing_with_empty_boards_and_a_placement_deadline()
    {
        using var h = new GameServiceHarness();

        var game = await h.CreateGameAsync();

        Assert.Equal(GameStatus.Preparing, game.Status);
        Assert.All(game.Players, p => Assert.Equal(0, BoardFactory.CountOccupied(p.BoardTiles)));
        Assert.All(game.Players, p => Assert.Equal(8, p.Tokens.Count));
        Assert.NotNull(game.TurnExpiresAt);
        Assert.InRange(game.TurnExpiresAt!.Value, DateTime.UtcNow.AddSeconds(55), DateTime.UtcNow.AddSeconds(61));
    }

    [Fact]
    public async Task Valid_layout_is_stored()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        var result = await h.Service.PlaceAllTokensAsync(game.Id, User1, TestData.StandardLayout());

        Assert.True(result!.Success);
        var player = (await h.Service.GetGameAsync(game.Id))!.Players.Single(p => p.UserId == User1);
        Assert.Equal(TestData.TotalPieceCells, BoardFactory.CountOccupied(player.BoardTiles));
        Assert.Equal(4, player.Tokens[3].PositionY);
        Assert.Equal(Direction.Right, player.Tokens[3].Direction);
    }

    [Fact]
    public async Task Placing_again_replaces_the_previous_layout_instead_of_adding_to_it()
    {
        // regression: a retry used to collide with the cells of the first attempt
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        await h.Service.PlaceAllTokensAsync(game.Id, User1, TestData.StandardLayout());
        var retry = await h.Service.PlaceAllTokensAsync(game.Id, User1, TestData.StandardLayout());
        var moved = await h.Service.PlaceAllTokensAsync(game.Id, User1, TestData.VerticalLayout());

        Assert.True(retry!.Success);
        Assert.True(moved!.Success);
        var board = (await h.Service.GetGameAsync(game.Id))!.Players.Single(p => p.UserId == User1).BoardTiles;
        Assert.Equal(TestData.TotalPieceCells, BoardFactory.CountOccupied(board));
        Assert.Equal("B", board[4][3]); // bull now runs down column 4
    }

    [Fact]
    public async Task Collision_fails_with_the_token_index_and_keeps_the_stored_board()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();
        var layout = TestData.StandardLayout();
        layout[7] = new TokenPlacement(7, 2, 4, Direction.Right); // cow on top of the bull

        var result = await h.Service.PlaceAllTokensAsync(game.Id, User1, layout);

        Assert.False(result!.Success);
        Assert.Equal(PlacementFailureReason.Collision, result.Reason);
        Assert.Equal(7, result.TokenIndex);
        var board = (await h.Service.GetGameAsync(game.Id))!.Players.Single(p => p.UserId == User1).BoardTiles;
        Assert.Equal(0, BoardFactory.CountOccupied(board));
    }

    [Fact]
    public async Task Out_of_bounds_fails_with_the_token_index()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();
        var layout = TestData.StandardLayout();
        layout[3] = new TokenPlacement(3, 8, 4, Direction.Right);

        var result = await h.Service.PlaceAllTokensAsync(game.Id, User1, layout);

        Assert.Equal(PlacementFailureReason.OutOfBounds, result!.Reason);
        Assert.Equal(3, result.TokenIndex);
    }

    [Fact]
    public async Task Missing_or_duplicated_tokens_are_rejected()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        var missing = TestData.StandardLayout().Take(7).ToList();
        var duplicated = TestData.StandardLayout();
        duplicated[1] = duplicated[0] with { PositionY = 9 };

        Assert.Equal(PlacementFailureReason.IncompleteSet, (await h.Service.PlaceAllTokensAsync(game.Id, User1, missing))!.Reason);
        Assert.Equal(PlacementFailureReason.IncompleteSet, (await h.Service.PlaceAllTokensAsync(game.Id, User1, duplicated))!.Reason);
    }

    [Fact]
    public async Task Big_piece_without_direction_is_rejected()
    {
        // a Centered bull used to be "placed" without writing any cell
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();
        var layout = TestData.StandardLayout();
        layout[3] = layout[3] with { Direction = Direction.Centered };

        var result = await h.Service.PlaceAllTokensAsync(game.Id, User1, layout);

        Assert.Equal(PlacementFailureReason.InvalidInput, result!.Reason);
        Assert.Equal(3, result.TokenIndex);
    }

    [Fact]
    public async Task Placement_is_locked_after_confirming()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreatePlacedGameAsync();
        await h.Service.SetPlayerReadyAsync(game.Id, User1);

        var result = await h.Service.PlaceAllTokensAsync(game.Id, User1, TestData.VerticalLayout());

        Assert.Equal(PlacementFailureReason.NotAllowed, result!.Reason);
    }

    [Fact]
    public async Task Placement_is_locked_during_the_battle()
    {
        // regression: moving pieces mid-battle could make a player impossible to defeat
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();

        foreach (var userId in new[] { User1, User2 })
        {
            var result = await h.Service.PlaceAllTokensAsync(game.Id, userId, TestData.VerticalLayout());
            Assert.Equal(PlacementFailureReason.NotAllowed, result!.Reason);
        }
    }

    [Fact]
    public async Task Unknown_game_or_player_returns_null()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        Assert.Null(await h.Service.PlaceAllTokensAsync("nope", User1, TestData.StandardLayout()));
        Assert.Null(await h.Service.PlaceAllTokensAsync(game.Id, "stranger", TestData.StandardLayout()));
    }
}

public class GameServiceReadyTests
{
    [Fact]
    public async Task Ready_without_placing_is_not_allowed()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        Assert.Equal(ReadyResult.NotAllowed, await h.Service.SetPlayerReadyAsync(game.Id, User1));
        Assert.Equal(PlayerStatus.Preparing, (await h.Service.GetGameAsync(game.Id))!.Players.Single(p => p.UserId == User1).PlayerStatus);
    }

    [Fact]
    public async Task Battle_starts_when_both_are_ready()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreatePlacedGameAsync();

        Assert.Equal(ReadyResult.WaitingOpponent, await h.Service.SetPlayerReadyAsync(game.Id, User1));
        Assert.Equal(ReadyResult.WaitingOpponent, await h.Service.SetPlayerReadyAsync(game.Id, User1));
        Assert.Equal(ReadyResult.BothReady, await h.Service.SetPlayerReadyAsync(game.Id, User2));

        game = (await h.Service.GetGameAsync(game.Id))!;
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.NotNull(game.CurrentTurnPlayerNum);
        Assert.InRange(game.TurnExpiresAt!.Value, DateTime.UtcNow.AddSeconds(25), DateTime.UtcNow.AddSeconds(31));
    }

    [Fact]
    public async Task Ready_again_after_the_battle_started_does_not_redraw_the_turn()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var turn = game.CurrentTurnPlayerNum;

        for (var i = 0; i < 10; i++) Assert.Equal(ReadyResult.NotAllowed, await h.Service.SetPlayerReadyAsync(game.Id, User1));

        Assert.Equal(turn, (await h.Service.GetGameAsync(game.Id))!.CurrentTurnPlayerNum);
    }

    [Fact]
    public async Task Placement_timeout_places_everyone_who_did_not_confirm_and_starts_the_battle()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        var autoPlaced = await h.Service.AutoCompletePlacementAsync(game.Id);

        Assert.Equal(["Alice", "Bob"], autoPlaced!.Order());
        game = (await h.Service.GetGameAsync(game.Id))!;
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.All(game.Players, p =>
        {
            Assert.Equal(PlayerStatus.Ready, p.PlayerStatus);
            Assert.Equal(TestData.TotalPieceCells, BoardFactory.CountOccupied(p.BoardTiles));
        });
    }

    [Fact]
    public async Task Placement_timeout_keeps_the_layout_of_whoever_confirmed()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();
        await h.Service.PlaceAllTokensAsync(game.Id, User1, TestData.StandardLayout());
        await h.Service.SetPlayerReadyAsync(game.Id, User1);
        var confirmedBoard = (await h.Service.GetGameAsync(game.Id))!.Players.Single(p => p.UserId == User1).BoardTiles
            .Select(r => r.ToArray()).ToArray();

        var autoPlaced = await h.Service.AutoCompletePlacementAsync(game.Id);

        Assert.Equal(["Bob"], autoPlaced);
        Assert.Equal(confirmedBoard, (await h.Service.GetGameAsync(game.Id))!.Players.Single(p => p.UserId == User1).BoardTiles);
    }

    [Fact]
    public async Task Placement_timeout_does_nothing_once_the_battle_started()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();

        Assert.Null(await h.Service.AutoCompletePlacementAsync(game.Id));
    }
}

public class GameServiceBattleTests
{
    [Fact]
    public async Task Only_the_current_player_can_attack()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();

        var result = await h.Service.AttackAsync(game.Id, OtherPlayer(game).UserId, 0, 0);

        Assert.Equal(AttackFailureReason.NotYourTurn, result!.Reason);
    }

    [Fact]
    public async Task Nobody_can_attack_during_placement()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreatePlacedGameAsync();

        Assert.Equal(AttackFailureReason.NotYourTurn, (await h.Service.AttackAsync(game.Id, User1, 0, 0))!.Reason);
        Assert.Equal(AttackFailureReason.NotYourTurn, (await h.Service.AttackAsync(game.Id, User2, 0, 0))!.Reason);
    }

    [Fact]
    public async Task A_miss_passes_the_turn_and_a_hit_keeps_it()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var attacker = CurrentPlayer(game);
        var defender = OtherPlayer(game);
        var target = TestData.OccupiedCells(defender.BoardTiles)[0];

        var hit = await h.Service.AttackAsync(game.Id, attacker.UserId, target.X, target.Y);
        Assert.True(hit!.Hit);
        Assert.Equal(attacker.PlayerNum, hit.NextTurnPlayerNum);

        var empty = TestData.FirstEmptyCell(defender.BoardTiles);
        var miss = await h.Service.AttackAsync(game.Id, attacker.UserId, empty.X, empty.Y);
        Assert.False(miss!.Hit);
        Assert.Equal(defender.PlayerNum, miss.NextTurnPlayerNum);
    }

    [Fact]
    public async Task Same_cell_and_out_of_bounds_are_rejected()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var attacker = CurrentPlayer(game);
        var target = TestData.OccupiedCells(OtherPlayer(game).BoardTiles)[0];

        await h.Service.AttackAsync(game.Id, attacker.UserId, target.X, target.Y);

        Assert.Equal(AttackFailureReason.AlreadyAttacked, (await h.Service.AttackAsync(game.Id, attacker.UserId, target.X, target.Y))!.Reason);
        Assert.Equal(AttackFailureReason.OutOfBounds, (await h.Service.AttackAsync(game.Id, attacker.UserId, 10, 0))!.Reason);
        Assert.Equal(AttackFailureReason.OutOfBounds, (await h.Service.AttackAsync(game.Id, attacker.UserId, 0, -1))!.Reason);
    }

    [Fact]
    public async Task Hitting_every_piece_cell_wins_the_game()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var attacker = CurrentPlayer(game);
        var defender = OtherPlayer(game);
        var cells = TestData.OccupiedCells(defender.BoardTiles);

        AttackResult? last = null;
        foreach (var (x, y) in cells) last = await h.Service.AttackAsync(game.Id, attacker.UserId, x, y);

        Assert.True(last!.GameEnded);
        Assert.Equal(attacker.PlayerNum, last.WinnerPlayerNum);
        game = (await h.Service.GetGameAsync(game.Id))!;
        Assert.Equal(GameStatus.Ended, game.Status);
        Assert.Null(game.CurrentTurnPlayerNum);
        Assert.Equal(PlayerStatus.Winner, game.Players.Single(p => p.UserId == attacker.UserId).PlayerStatus);
        Assert.Equal(PlayerStatus.Loser, game.Players.Single(p => p.UserId == defender.UserId).PlayerStatus);
    }
}

public class GameServiceClockTests
{
    [Fact]
    public async Task Timeout_passes_the_turn()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var idle = CurrentPlayer(game);
        var other = OtherPlayer(game);

        var result = await h.Service.TimeoutTurnAsync(game.Id);

        Assert.False(result!.GameEnded);
        Assert.Equal(other.PlayerNum, result.NextTurnPlayerNum);
        Assert.Equal(1, (await h.Service.GetGameAsync(game.Id))!.Players.Single(p => p.UserId == idle.UserId).ConsecutiveTimeouts);
    }

    [Fact]
    public async Task Third_consecutive_timeout_loses_the_game()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var idle = CurrentPlayer(game);
        var other = OtherPlayer(game);

        // idle, other, idle, other, idle -> idle hits 3 first
        AttackResult? result = null;
        for (var i = 0; i < 5; i++) result = await h.Service.TimeoutTurnAsync(game.Id);

        Assert.True(result!.GameEnded);
        Assert.Equal(other.PlayerNum, result.WinnerPlayerNum);
        game = (await h.Service.GetGameAsync(game.Id))!;
        Assert.Equal(GameStatus.Ended, game.Status);
        Assert.Equal(PlayerStatus.Loser, game.Players.Single(p => p.UserId == idle.UserId).PlayerStatus);
        Assert.Null(await h.Service.TimeoutTurnAsync(game.Id));
    }

    [Fact]
    public async Task Attacking_resets_the_timeout_count()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var first = CurrentPlayer(game);
        var second = OtherPlayer(game);

        await h.Service.TimeoutTurnAsync(game.Id);   // first: 1
        await h.Service.TimeoutTurnAsync(game.Id);   // second: 1
        await h.Service.TimeoutTurnAsync(game.Id);   // first: 2

        // second plays a miss -> back to first, whose count must keep counting from 2...
        var empty = TestData.FirstEmptyCell(first.BoardTiles);
        await h.Service.AttackAsync(game.Id, second.UserId, empty.X, empty.Y);

        // ...but first now plays too, which resets them
        var target = TestData.FirstEmptyCell(second.BoardTiles);
        await h.Service.AttackAsync(game.Id, first.UserId, target.X, target.Y);

        game = (await h.Service.GetGameAsync(game.Id))!;
        Assert.All(game.Players, p => Assert.Equal(0, p.ConsecutiveTimeouts));
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public async Task Reconnecting_on_your_own_turn_keeps_the_remaining_time()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        var current = CurrentPlayer(game);
        var deadline = DateTime.UtcNow.AddSeconds(12);
        game.TurnExpiresAt = deadline;
        await h.Context.SaveChangesAsync();

        var refreshed = await h.Service.RefreshTurnDeadlineAsync(game.Id, current.UserId);

        Assert.Equal(deadline, refreshed);
    }

    [Fact]
    public async Task Reconnecting_on_your_own_turn_with_the_clock_almost_out_gets_the_grace()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        game.TurnExpiresAt = DateTime.UtcNow.AddSeconds(-20);
        await h.Context.SaveChangesAsync();

        var refreshed = await h.Service.RefreshTurnDeadlineAsync(game.Id, CurrentPlayer(game).UserId);

        Assert.InRange(refreshed!.Value, DateTime.UtcNow.AddSeconds(3), DateTime.UtcNow.AddSeconds(6));
    }

    [Fact]
    public async Task Reconnecting_on_the_opponents_turn_gives_the_opponent_a_full_turn()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();
        game.TurnExpiresAt = DateTime.UtcNow.AddSeconds(2);
        await h.Context.SaveChangesAsync();

        var refreshed = await h.Service.RefreshTurnDeadlineAsync(game.Id, OtherPlayer(game).UserId);

        Assert.InRange(refreshed!.Value, DateTime.UtcNow.AddSeconds(28), DateTime.UtcNow.AddSeconds(31));
    }

    [Fact]
    public async Task No_turn_clock_outside_the_battle()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        Assert.Null(await h.Service.RefreshTurnDeadlineAsync(game.Id, User1));
        Assert.Null(await h.Service.TimeoutTurnAsync(game.Id));
    }
}

public class GameServiceEndingTests
{
    [Fact]
    public async Task Forfeit_gives_the_win_to_the_other_player_once()
    {
        using var h = new GameServiceHarness();
        var game = await h.StartBattleAsync();

        var result = await h.Service.ForfeitAsync(game.Id, User1);

        Assert.True(result!.GameEnded);
        game = (await h.Service.GetGameAsync(game.Id))!;
        Assert.Equal(GameStatus.Ended, game.Status);
        Assert.Equal(PlayerStatus.Winner, game.Players.Single(p => p.UserId == User2).PlayerStatus);
        Assert.Null(await h.Service.ForfeitAsync(game.Id, User2));
    }

    [Fact]
    public async Task Forfeit_works_during_placement()
    {
        using var h = new GameServiceHarness();
        var game = await h.CreateGameAsync();

        var result = await h.Service.ForfeitAsync(game.Id, User2);

        Assert.Equal(PlayerNum.Player1, result!.WinnerPlayerNum);
    }

    [Fact]
    public async Task Startup_cleanup_cancels_only_unfinished_games()
    {
        // ExecuteUpdate needs a relational provider, so this one runs on SQLite
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
        var service = new GameService(context, new GameLocks(), new BatalhaRuralTimings());

        var preparing = await service.CreateGameAsync(User1, "Alice", User2, "Bob");
        var inProgress = await service.CreateGameAsync("u3", "Carol", "u4", "Dave");
        var ended = await service.CreateGameAsync("u5", "Eve", "u6", "Frank");
        foreach (var userId in new[] { "u3", "u4" })
        {
            await service.PlaceAllTokensAsync(inProgress.Id, userId, TestData.StandardLayout());
            await service.SetPlayerReadyAsync(inProgress.Id, userId);
        }
        await service.ForfeitAsync(ended.Id, "u5");

        var cancelled = await service.CancelUnfinishedGamesAsync();

        Assert.Equal(2, cancelled);
        context.ChangeTracker.Clear();
        Assert.Equal(GameStatus.Cancelled, (await service.GetGameAsync(preparing.Id))!.Status);
        Assert.Equal(GameStatus.Cancelled, (await service.GetGameAsync(inProgress.Id))!.Status);
        Assert.Null((await service.GetGameAsync(inProgress.Id))!.CurrentTurnPlayerNum);
        Assert.Equal(GameStatus.Ended, (await service.GetGameAsync(ended.Id))!.Status);
    }
}
