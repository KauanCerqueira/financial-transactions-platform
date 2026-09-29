using FinancialTransactions.Application.Abstractions.Caching;

namespace FinancialTransactions.UnitTests.TestDoubles;

public sealed class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, object> entries = [];

    public bool Contains(string key) => entries.ContainsKey(key);

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = entries.TryGetValue(key, out var stored) ? (T?)stored : default;

        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        entries[key] = value!;

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        entries.Remove(key);

        return Task.CompletedTask;
    }
}
