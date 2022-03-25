using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace Ddon.Cache
{
    public class MemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool ContainsKey(object key)
        {
            return _memoryCache.Get(key) is not null;
        }

        public object? Get(object key)
        {
            return _memoryCache.Get(key);
        }

        public TItem Get<TItem>(object key)
        {
            return _memoryCache.Get<TItem>(key);
        }

        public void Remove(object key)
        {
            _memoryCache.Remove(key);
        }

        public void Remove(object[] keys)
        {
            Parallel.ForEach(keys, Remove);
        }

        public TItem Set<TItem>(object key, TItem value)
        {
            return _memoryCache.Set(key, value);
        }

        public TItem Set<TItem>(object key, TItem value, MemoryCacheEntryOptions options)
        {
            return _memoryCache.Set(key, value, options);
        }

        public TItem Set<TItem>(object key, TItem value, IChangeToken expirationToken)
        {
            return _memoryCache.Set(key, value, expirationToken);
        }

        public TItem Set<TItem>(object key, TItem value, DateTimeOffset absoluteExpiration)
        {
            return _memoryCache.Set(key, value, absoluteExpiration);
        }

        public TItem Set<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        {
            return _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
        }
    }
}
