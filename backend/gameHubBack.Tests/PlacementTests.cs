using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.Tests;

public class PlacementHelperTests
{
    private static GameToken Token(TokenType type, int x, int y, Direction direction) =>
        new() { Type = type, PositionX = x, PositionY = y, Direction = direction };

    [Theory]
    [InlineData(Direction.Right, 3, 3, new[] { 2, 3, 4, 5, 6 }, new[] { 2, 2, 2, 2, 2 })]
    [InlineData(Direction.Left, 7, 3, new[] { 6, 5, 4, 3, 2 }, new[] { 2, 2, 2, 2, 2 })]
    [InlineData(Direction.Down, 3, 3, new[] { 2, 2, 2, 2, 2 }, new[] { 2, 3, 4, 5, 6 })]
    [InlineData(Direction.Up, 3, 7, new[] { 2, 2, 2, 2, 2 }, new[] { 6, 5, 4, 3, 2 })]
    public void Bull_occupies_five_cells_in_its_direction(Direction direction, int x, int y, int[] xs, int[] ys)
    {
        var board = BoardFactory.CreateEmpty();

        var result = PlacementHelper.TryPlaceToken(Token(TokenType.Bull, x, y, direction), board);

        Assert.True(result.Success);
        Assert.Equal(5, BoardFactory.CountOccupied(board));
        for (var i = 0; i < xs.Length; i++) Assert.Equal("B", board[ys[i]][xs[i]]);
    }

    [Fact]
    public void Chicken_occupies_a_single_cell()
    {
        var board = BoardFactory.CreateEmpty();

        Assert.True(PlacementHelper.TryPlaceToken(Token(TokenType.Chicken, 10, 10, Direction.Centered), board).Success);
        Assert.Equal("c", board[9][9]);
        Assert.Equal(1, BoardFactory.CountOccupied(board));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(11, 1)]
    [InlineData(1, 11)]
    public void Position_outside_the_board_is_invalid_input(int x, int y)
    {
        var board = BoardFactory.CreateEmpty();

        var result = PlacementHelper.TryPlaceToken(Token(TokenType.Chicken, x, y, Direction.Centered), board);

        Assert.Equal(PlacementFailureReason.InvalidInput, result.Reason);
        Assert.Equal(0, BoardFactory.CountOccupied(board));
    }

    [Theory]
    [InlineData(8, 1, Direction.Right)]
    [InlineData(3, 1, Direction.Left)]
    [InlineData(1, 8, Direction.Down)]
    [InlineData(1, 3, Direction.Up)]
    public void Piece_running_off_the_edge_is_out_of_bounds(int x, int y, Direction direction)
    {
        var board = BoardFactory.CreateEmpty();

        var result = PlacementHelper.TryPlaceToken(Token(TokenType.Bull, x, y, direction), board);

        Assert.Equal(PlacementFailureReason.OutOfBounds, result.Reason);
        Assert.Equal(0, BoardFactory.CountOccupied(board));
    }

    [Fact]
    public void Piece_starting_on_an_occupied_cell_collides()
    {
        var board = BoardFactory.CreateEmpty();
        PlacementHelper.TryPlaceToken(Token(TokenType.Chicken, 5, 5, Direction.Centered), board);

        var result = PlacementHelper.TryPlaceToken(Token(TokenType.Cow, 5, 5, Direction.Right), board);

        Assert.Equal(PlacementFailureReason.Collision, result.Reason);
    }

    [Theory]
    [InlineData(Direction.Right, 3, 5)]
    [InlineData(Direction.Left, 7, 5)]
    [InlineData(Direction.Down, 5, 3)]
    [InlineData(Direction.Up, 5, 7)]
    public void Piece_whose_path_crosses_another_collides(Direction direction, int x, int y)
    {
        var board = BoardFactory.CreateEmpty();
        PlacementHelper.TryPlaceToken(Token(TokenType.Chicken, 5, 5, Direction.Centered), board);

        var result = PlacementHelper.TryPlaceToken(Token(TokenType.Bull, x, y, direction), board);

        Assert.Equal(PlacementFailureReason.Collision, result.Reason);
        Assert.Equal(1, BoardFactory.CountOccupied(board));
    }
}

public class RandomPlacerTests
{
    [Fact]
    public void Random_layouts_are_always_complete_and_valid()
    {
        var random = new Random(1234);

        for (var run = 0; run < 300; run++)
        {
            var (board, tokens) = RandomPlacer.Place(TokenCatalog.CreateDefaultTokens(), BoardFactory.DefaultSize, random);

            Assert.Equal(TestData.TotalPieceCells, BoardFactory.CountOccupied(board));
            Assert.Equal(TokenCatalog.CreateDefaultTokens().Select(t => t.Type), tokens.Select(t => t.Type));

            // replaying the chosen positions on an empty board must rebuild the same board,
            // i.e. the layout is one the manual placement rules accept
            var replay = BoardFactory.CreateEmpty();
            foreach (var token in tokens) Assert.True(PlacementHelper.TryPlaceToken(token, replay).Success);
            Assert.Equal(board, replay);
        }
    }

    [Fact]
    public void Random_layout_is_accepted_by_PlaceAllTokens()
    {
        // the server must accept a random layout coming back as a manual placement
        using var harness = new GameServiceHarness();
        var game = harness.CreateGameAsync().GetAwaiter().GetResult();
        var (_, tokens) = RandomPlacer.Place(TokenCatalog.CreateDefaultTokens(), BoardFactory.DefaultSize, new Random(42));

        var placements = tokens.Select((t, i) => new TokenPlacement(i, t.PositionX, t.PositionY, t.Direction)).ToList();
        var result = harness.Service.PlaceAllTokensAsync(game.Id, GameServiceHarness.User1, placements).GetAwaiter().GetResult();

        Assert.True(result!.Success);
    }
}
