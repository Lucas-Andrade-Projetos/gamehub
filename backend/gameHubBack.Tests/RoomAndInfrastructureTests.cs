using gameHubBack.Services;
using gameHubBack.Services.BatalhaRural;
using gameHubBack.Services.Rooms;

namespace gameHubBack.Tests;

public class RoomRegistryTests
{
    private static (RoomRegistry Registry, GameRoom Room) FullRoom()
    {
        var registry = new RoomRegistry();
        var room = registry.CreateRoom("u1", "Alice", "c1");
        registry.TryJoinRoom(room.Code, "u2", "Bob", "c2");
        return (registry, room);
    }

    [Fact]
    public void Room_holds_two_players_and_leaves_the_open_list_when_full()
    {
        var registry = new RoomRegistry();
        var room = registry.CreateRoom("u1", "Alice", "c1");

        Assert.Single(registry.GetOpenRooms());
        Assert.NotNull(registry.TryJoinRoom(room.Code, "u2", "Bob", "c2"));
        Assert.Null(registry.TryJoinRoom(room.Code, "u3", "Carol", "c3"));
        Assert.Empty(registry.GetOpenRooms());
    }

    [Fact]
    public void Room_is_found_by_connection()
    {
        var (registry, room) = FullRoom();

        Assert.Same(room, registry.GetRoomByConnection("c2"));
        Assert.Null(registry.GetRoomByConnection("unknown"));
    }

    [Fact]
    public void Both_ready_is_reported()
    {
        var (registry, room) = FullRoom();

        Assert.False(registry.SetReady(room.Code, "u1", true));
        Assert.True(registry.SetReady(room.Code, "u2", true));
        Assert.False(registry.SetReady(room.Code, "stranger", true));
    }

    [Fact]
    public void Disconnect_in_the_lobby_frees_the_slot_and_empty_rooms_are_removed()
    {
        var (registry, room) = FullRoom();

        registry.HandleDisconnect("c2");
        Assert.Null(room.Player2);

        Assert.Null(registry.HandleDisconnect("c1"));
        Assert.Null(registry.GetRoom(room.Code));
    }

    [Fact]
    public void Disconnect_during_a_match_keeps_the_slot_for_reconnection()
    {
        var (registry, room) = FullRoom();
        registry.SetGameId(room.Code, "game-1");

        registry.HandleDisconnect("c2");

        Assert.False(room.Player2!.Connected);
        Assert.Same(room, registry.TryRejoin(room.Code, "u2", "c2b"));
        Assert.True(room.Player2.Connected);
        Assert.Same(room, registry.GetRoomByConnection("c2b"));
    }

    [Fact]
    public void Reset_drops_players_who_never_came_back()
    {
        var (registry, room) = FullRoom();
        registry.SetGameId(room.Code, "game-1");
        registry.HandleDisconnect("c2");

        var reset = registry.ResetRoom(room.Code);

        Assert.NotNull(reset);
        Assert.Null(reset!.GameId);
        Assert.Null(reset.Player2);
        Assert.Single(registry.GetOpenRooms());
    }

    [Fact]
    public void Abandoned_room_is_removed_only_when_nobody_is_connected()
    {
        var (registry, room) = FullRoom();
        registry.SetGameId(room.Code, "game-1");

        registry.HandleDisconnect("c1");
        Assert.False(registry.RemoveRoomIfAbandoned(room.Code));

        registry.HandleDisconnect("c2");
        Assert.True(registry.RemoveRoomIfAbandoned(room.Code));
        Assert.Null(registry.GetRoom(room.Code));
    }

    [Fact]
    public void Rematch_starts_only_when_both_vote_and_clears_the_votes()
    {
        var (registry, room) = FullRoom();

        Assert.False(registry.RequestRematch(room.Code, "u1"));
        Assert.False(registry.RequestRematch(room.Code, "u1"));
        Assert.True(room.Player1!.WantsRematch);

        Assert.True(registry.RequestRematch(room.Code, "u2"));
        Assert.False(room.Player1.WantsRematch);
        Assert.False(room.Player2!.WantsRematch);

        Assert.False(registry.RequestRematch(room.Code, "stranger"));
    }

    [Fact]
    public async Task Simultaneous_rematch_votes_start_exactly_one_game()
    {
        for (var run = 0; run < 200; run++)
        {
            var (registry, room) = FullRoom();
            using var start = new ManualResetEventSlim();

            var votes = new[] { "u1", "u2" }
                .Select(userId => Task.Run(() => { start.Wait(); return registry.RequestRematch(room.Code, userId); }))
                .ToArray();
            start.Set();

            var results = await Task.WhenAll(votes);
            Assert.Equal(1, results.Count(r => r));
        }
    }
}

public class GameLocksTests
{
    [Fact]
    public async Task Actions_on_the_same_game_never_overlap()
    {
        var locks = new GameLocks();
        var running = 0;
        var maxConcurrent = 0;

        await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => locks.RunAsync("game", async () =>
        {
            var now = Interlocked.Increment(ref running);
            maxConcurrent = Math.Max(maxConcurrent, now);
            await Task.Delay(2);
            Interlocked.Decrement(ref running);
            return 0;
        })));

        Assert.Equal(1, maxConcurrent);
    }

    [Fact]
    public async Task Different_games_run_in_parallel()
    {
        var locks = new GameLocks();
        var gate = new TaskCompletionSource();

        // game A waits for game B; if they shared a lock this would deadlock
        var a = locks.RunAsync("a", async () => { await gate.Task; return 1; });
        var b = locks.RunAsync("b", () => { gate.SetResult(); return Task.FromResult(2); });

        var finished = await Task.WhenAny(Task.WhenAll(a, b), Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.True(a.IsCompletedSuccessfully && b.IsCompletedSuccessfully, $"finished: {finished}");
    }

    [Fact]
    public async Task Lock_is_usable_again_after_an_action_throws()
    {
        var locks = new GameLocks();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            locks.RunAsync<int>("game", () => throw new InvalidOperationException()));

        Assert.Equal(7, await locks.RunAsync("game", () => Task.FromResult(7)));
    }
}

public class DelayedActionSchedulerTests
{
    [Fact]
    public async Task Scheduled_action_runs_after_the_delay()
    {
        var scheduler = new DelayedActionScheduler();
        var ran = new TaskCompletionSource();

        scheduler.Schedule("k", TimeSpan.FromMilliseconds(50), () => { ran.SetResult(); return Task.CompletedTask; });

        Assert.Same(ran.Task, await Task.WhenAny(ran.Task, Task.Delay(2000)));
    }

    [Fact]
    public async Task Cancelled_action_never_runs()
    {
        var scheduler = new DelayedActionScheduler();
        var ran = false;

        scheduler.Schedule("k", TimeSpan.FromMilliseconds(50), () => { ran = true; return Task.CompletedTask; });
        scheduler.Cancel("k");
        await Task.Delay(200);

        Assert.False(ran);
    }

    [Fact]
    public async Task Rescheduling_a_key_replaces_the_previous_action()
    {
        var scheduler = new DelayedActionScheduler();
        var runs = new List<string>();

        scheduler.Schedule("k", TimeSpan.FromMilliseconds(50), () => { lock (runs) runs.Add("first"); return Task.CompletedTask; });
        scheduler.Schedule("k", TimeSpan.FromMilliseconds(80), () => { lock (runs) runs.Add("second"); return Task.CompletedTask; });
        await Task.Delay(300);

        Assert.Equal(["second"], runs);
    }

    [Fact]
    public async Task Action_can_reschedule_its_own_key()
    {
        // the turn timer reschedules itself from inside its own callback
        var scheduler = new DelayedActionScheduler();
        var count = 0;
        var done = new TaskCompletionSource();

        void Tick() => scheduler.Schedule("k", TimeSpan.FromMilliseconds(20), () =>
        {
            if (Interlocked.Increment(ref count) < 3) Tick();
            else done.SetResult();
            return Task.CompletedTask;
        });

        Tick();

        Assert.Same(done.Task, await Task.WhenAny(done.Task, Task.Delay(2000)));
        Assert.Equal(3, count);
    }
}
