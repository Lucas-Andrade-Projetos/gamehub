namespace gameHubBack.Services.BatalhaRural;

// One lock per game. Each entry is reference-counted and removed as soon as nobody holds or
// waits on it, so finished games don't leave semaphores behind forever.
public class GameLocks
{
    private readonly object _sync = new();
    private readonly Dictionary<string, Entry> _locks = new();

    public async Task<T> RunAsync<T>(string gameId, Func<Task<T>> action)
    {
        Entry entry;

        lock (_sync)
        {
            if (!_locks.TryGetValue(gameId, out entry!))
            {
                entry = new Entry();
                _locks[gameId] = entry;
            }

            entry.Users++;
        }

        await entry.Gate.WaitAsync();

        try
        {
            return await action();
        }
        finally
        {
            entry.Gate.Release();

            lock (_sync)
            {
                entry.Users--;

                if (entry.Users == 0)
                {
                    _locks.Remove(gameId);
                    entry.Gate.Dispose();
                }
            }
        }
    }

    private sealed class Entry
    {
        public SemaphoreSlim Gate { get; } = new(1, 1);
        public int Users { get; set; }
    }
}
