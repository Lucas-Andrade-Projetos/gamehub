namespace gameHubBack.Services.BatalhaRural;

public enum PlacementFailureReason
{
    InvalidInput,
    OutOfBounds,
    Collision
}

public class PlacementResult
{
    public required bool Success { get; init; }
    public PlacementFailureReason? Reason { get; init; }

    public static PlacementResult Ok() => new() { Success = true };

    public static PlacementResult Fail(PlacementFailureReason reason) => new() { Success = false, Reason = reason };
}