using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Common.Constants
{
    public static class CacheEntry
    {
        private static readonly TimeSpan SlidingExpiration = TimeSpan.FromSeconds(30);
        private const int EntrySize = 4096;
        public static readonly MemoryCacheEntryOptions BaseEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(SlidingExpiration)
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(EntrySize);
    }
}
