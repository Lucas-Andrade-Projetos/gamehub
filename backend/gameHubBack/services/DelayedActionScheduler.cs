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

            if (token.IsCancellationRequested) return;

            // drop our own entry before running, so finished timers don't pile up in the
            // dictionary. Only removes it if it's still ours - if the action reschedules the
            // same key, that new timer is left alone.
            if (_timers.TryRemove(new KeyValuePair<string, CancellationTokenSource>(key, cts))) cts.Dispose();

            await action();
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
