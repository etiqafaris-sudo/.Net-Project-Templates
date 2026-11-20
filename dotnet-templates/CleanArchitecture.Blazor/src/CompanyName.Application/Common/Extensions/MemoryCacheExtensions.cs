using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Common.Extensions;

public static class MemoryCacheExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FactoryLocks = new(StringComparer.Ordinal);

    /// <summary>
    /// Ensures only one cache factory runs per key at a time. Use when the factory uses a scoped DbContext.
    /// </summary>
    public static async Task<TItem?> GetOrCreateExclusiveAsync<TItem>(
        this IMemoryCache cache,
        object key,
        Func<ICacheEntry, Task<TItem>> factory,
        MemoryCacheEntryOptions? options = null)
    {
        if (cache.TryGetValue(key, out TItem? cached))
        {
            return cached;
        }

        var lockKey = key.ToString() ?? key.GetHashCode().ToString();
        var gate = FactoryLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            if (cache.TryGetValue(key, out cached))
            {
                return cached;
            }

            return await cache.GetOrCreateAsync(key, factory, options);
        }
        finally
        {
            gate.Release();
        }
    }
}
