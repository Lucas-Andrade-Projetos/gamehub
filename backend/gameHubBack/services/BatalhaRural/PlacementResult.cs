using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Services.BatalhaRural;

public enum PlacementFailureReason
{
    InvalidInput,
    OutOfBounds,
    Collision,
    NotAllowed,
    IncompleteSet
}

public class PlacementResult
{
    public required bool Success { get; init; }
    public PlacementFailureReason? Reason { get; init; }
    public int? TokenIndex { get; init; }

    public static PlacementResult Ok() => new() { Success = true };

    public static PlacementResult Fail(PlacementFailureReason reason, int? tokenIndex = null) =>
        new() { Success = false, Reason = reason, TokenIndex = tokenIndex };
}

public record TokenPlacement(int Index, int PositionX, int PositionY, Direction Direction);

public enum ReadyResult
{
    NotAllowed,
    WaitingOpponent,
    BothReady
}
