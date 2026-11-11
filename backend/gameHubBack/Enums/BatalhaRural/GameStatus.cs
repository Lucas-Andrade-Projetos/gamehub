namespace gameHubBack.Enums.BatalhaRural;

public enum GameStatus
{
    Preparing,
    InProgress,
    Ended,
    // game interrupted without a result (e.g. the server restarted mid-match)
    Cancelled
}
