using System.Collections.Concurrent;

namespace gameHubBack.Services.BatalhaRural;

public class GameLocks
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<T> RunAsync<T>(string gameId, Func<Task<T>> action)
    {
        var gate = _locks.GetOrAdd(gameId, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync();

        try
        {
            return await action();
        }
        finally
        {
            gate.Release();
        }
    }
}
