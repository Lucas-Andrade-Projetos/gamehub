using gameHubBack.Data;
using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural;
using Microsoft.EntityFrameworkCore;

namespace gameHubBack.Tests;

public static class TestData
{
    // CreateDefaultTokens order: Chicken, Chicken, Chicken, Bull(5), Cow(2), Horse(2), Horse(2), Cow(2)
    public const int TotalPieceCells = 3 * 1 + 5 + 4 * 2;

    // one token per row, starting at column 1 and extending right
    public static List<TokenPlacement> StandardLayout() =>
    [
        new(0, 1, 1, Direction.Centered),
        new(1, 1, 2, Direction.Centered),
        new(2, 1, 3, Direction.Centered),
        new(3, 1, 4, Direction.Right),
        new(4, 1, 5, Direction.Right),
        new(5, 1, 6, Direction.Right),
        new(6, 1, 7, Direction.Right),
        new(7, 1, 8, Direction.Right),
    ];

    // same pieces, different spot: one token per column, starting at row 1 and extending down
    public static List<TokenPlacement> VerticalLayout() =>
    [
        new(0, 1, 1, Direction.Centered),
        new(1, 2, 1, Direction.Centered),
        new(2, 3, 1, Direction.Centered),
        new(3, 4, 1, Direction.Down),
        new(4, 5, 1, Direction.Down),
        new(5, 6, 1, Direction.Down),
        new(6, 7, 1, Direction.Down),
        new(7, 8, 1, Direction.Down),
    ];

    public static List<(int X, int Y)> OccupiedCells(string[][] board)
    {
        var cells = new List<(int X, int Y)>();

        for (var y = 0; y < board.Length; y++)
            for (var x = 0; x < board[y].Length; x++)
                if (board[y][x] != BoardFactory.EmptyTile) cells.Add((x, y));

        return cells;
    }

    public static (int X, int Y) FirstEmptyCell(string[][] board)
    {
        for (var y = 0; y < board.Length; y++)
            for (var x = 0; x < board[y].Length; x++)
                if (board[y][x] == BoardFactory.EmptyTile) return (x, y);

        throw new InvalidOperationException("Board is full");
    }
}

// A GameService backed by its own in-memory database.
public sealed class GameServiceHarness : IDisposable
{
    public const string User1 = "user-1";
    public const string User2 = "user-2";

    public AppDbContext Context { get; }
    public GameService Service { get; }
    public BatalhaRuralTimings Timings { get; }

    public GameServiceHarness(BatalhaRuralTimings? timings = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Timings = timings ?? new BatalhaRuralTimings();
        Context = new AppDbContext(options);
        Service = new GameService(Context, new GameLocks(), Timings);
    }

    public Task<BatalhaRuralGame> CreateGameAsync() => Service.CreateGameAsync(User1, "Alice", User2, "Bob");

    public async Task<BatalhaRuralGame> CreatePlacedGameAsync()
    {
        var game = await CreateGameAsync();

        await Service.PlaceAllTokensAsync(game.Id, User1, TestData.StandardLayout());
        await Service.PlaceAllTokensAsync(game.Id, User2, TestData.StandardLayout());

        return game;
    }

    public async Task<BatalhaRuralGame> StartBattleAsync()
    {
        var game = await CreatePlacedGameAsync();

        await Service.SetPlayerReadyAsync(game.Id, User1);
        await Service.SetPlayerReadyAsync(game.Id, User2);

        return (await Service.GetGameAsync(game.Id))!;
    }

    public static BatalhaRuralPlayer CurrentPlayer(BatalhaRuralGame game) =>
        game.Players.Single(p => p.PlayerNum == game.CurrentTurnPlayerNum);

    public static BatalhaRuralPlayer OtherPlayer(BatalhaRuralGame game) =>
        game.Players.Single(p => p.PlayerNum != game.CurrentTurnPlayerNum);

    public void Dispose() => Context.Dispose();
}
