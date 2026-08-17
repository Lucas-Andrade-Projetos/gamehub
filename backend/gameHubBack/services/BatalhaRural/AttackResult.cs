using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Services.BatalhaRural;

public enum AttackFailureReason
{
    NotYourTurn,
    OutOfBounds,
    AlreadyAttacked
}

public class AttackResult
{
    public required bool Success { get; init; }
    public AttackFailureReason? Reason { get; init; }
    public bool Hit { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public PlayerNum AttackerPlayerNum { get; init; }
    public PlayerNum DefenderPlayerNum { get; init; }
    public bool GameEnded { get; init; }
    public PlayerNum? NextTurnPlayerNum { get; init; }
    public PlayerNum? WinnerPlayerNum { get; init; }

    public static AttackResult Fail(AttackFailureReason reason) => new() { Success = false, Reason = reason };
}
