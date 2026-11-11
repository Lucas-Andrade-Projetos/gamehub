using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Services.BatalhaRural;

// Places a whole token set at random positions, using the same PlacementHelper rules as a
// manual placement - so a random layout is always one a player could have built by hand.
public static class RandomPlacer
{
    private static readonly Direction[] Directions = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
    private const int MaxLayoutAttempts = 100;
    private const int MaxTokenAttempts = 200;

    public static (string[][] Board, List<GameToken> Tokens) Place(IReadOnlyList<GameToken> tokens, int boardSize, Random random)
    {
        var expectedCells = tokens.Sum(t => TokenCatalog.GetSize(t.Type));

        // biggest pieces first: they're the hardest to fit once the board fills up
        var order = Enumerable.Range(0, tokens.Count)
            .OrderByDescending(i => TokenCatalog.GetSize(tokens[i].Type))
            .ToList();

        for (var layout = 0; layout < MaxLayoutAttempts; layout++)
        {
            var board = BoardFactory.CreateEmpty(boardSize);
            var placed = new GameToken[tokens.Count];
            var complete = true;

            foreach (var index in order)
            {
                var token = TryPlaceRandomly(tokens[index].Type, board, boardSize, random);

                if (token == null)
                {
                    complete = false;
                    break;
                }

                placed[index] = token;
            }

            if (complete && BoardFactory.CountOccupied(board) == expectedCells) return (board, placed.ToList());
        }

        throw new InvalidOperationException("Could not find a random layout for the token set");
    }

    private static GameToken? TryPlaceRandomly(TokenType type, string[][] board, int boardSize, Random random)
    {
        var size = TokenCatalog.GetSize(type);

        for (var attempt = 0; attempt < MaxTokenAttempts; attempt++)
        {
            var candidate = new GameToken
            {
                Type = type,
                PositionX = random.Next(1, boardSize + 1),
                PositionY = random.Next(1, boardSize + 1),
                Direction = size == 1 ? Direction.Centered : Directions[random.Next(Directions.Length)]
            };

            // TryPlaceToken writes into the board on success, so try on a copy and only keep
            // it when the placement is valid and wrote exactly the token's cells
            var copy = board.Select(row => row.ToArray()).ToArray();
            var before = BoardFactory.CountOccupied(copy);

            if (!PlacementHelper.TryPlaceToken(candidate, copy).Success) continue;
            if (BoardFactory.CountOccupied(copy) - before != size) continue;

            for (var y = 0; y < boardSize; y++) board[y] = copy[y];

            return candidate;
        }

        return null;
    }
}
