using System.Collections.Concurrent;

namespace gameHubBack.Services;

public class DelayedActionScheduler
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _timers = new();

    public void Schedule(string key, TimeSpan delay, Func<Task> action)
    {
        var cts = new CancellationTokenSource();

        // AddOrUpdate resolves same-key races atomically: whichever caller's update delegate
        // runs is handed whatever the other caller just stored, so the loser's CTS is always
        // the one cancelled - no timer is ever silently orphaned (uncancellable but still running).
        _timers.AddOrUpdate(key, cts, (_, existing) =>
        {
            existing.Cancel();
            existing.Dispose();
            return cts;
        });

        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            if (!token.IsCancellationRequested) await action();
        });
    }

    public void Cancel(string key)
    {
        if (_timers.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
