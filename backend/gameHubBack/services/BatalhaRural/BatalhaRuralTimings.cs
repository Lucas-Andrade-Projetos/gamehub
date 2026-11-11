namespace gameHubBack.Services.BatalhaRural;

// Every clock in a Batalha Rural match, in one place. Registered as a singleton so tests can
// swap in short durations instead of waiting real minutes.
public class BatalhaRuralTimings
{
    public TimeSpan TurnDuration { get; init; } = TimeSpan.FromSeconds(30);

    // time both players get to place their pieces before the server places them randomly
    public TimeSpan PlacementDuration { get; init; } = TimeSpan.FromMinutes(1);

    // how long a dropped player has to reconnect before forfeiting
    public TimeSpan ReconnectGrace { get; init; } = TimeSpan.FromSeconds(30);

    // minimum time left on the clock for a player who reconnects during their own turn
    public TimeSpan RejoinGrace { get; init; } = TimeSpan.FromSeconds(5);

    // a room nobody is connected to anymore is dropped after this
    public TimeSpan RoomCleanupDelay { get; init; } = TimeSpan.FromSeconds(35);

    public int MaxConsecutiveTimeouts { get; init; } = 3;
}
