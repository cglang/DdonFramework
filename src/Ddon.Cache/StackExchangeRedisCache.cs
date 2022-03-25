using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;

namespace Ddon.Cache
{
    public class StackExchangeRedisCache : RedisCache, ICache
    {
        private readonly IDistributedCache _distributedCache;

        public StackExchangeRedisCache(IOptions<RedisCacheOptions> optionsAccessor, IDistributedCache distributedCache) : base(optionsAccessor)
        {
            _distributedCache = distributedCache;
        }

        public bool ContainsKey(object key)
        {
            throw new NotImplementedException();
        }

        public object Get(object key)
        {
            throw new NotImplementedException();
        }

        public TItem? Get<TItem>(object key)
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public void Remove(object[] keys)
        {
            throw new NotImplementedException();
        }

        public TItem Set<TItem>(object key, TItem value)
        {
            throw new NotImplementedException();
        }

        public TItem Set<TItem>(object key, TItem value, MemoryCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }

        public TItem Set<TItem>(object key, TItem value, IChangeToken expirationToken)
        {
            throw new NotImplementedException();
        }

        public TItem Set<TItem>(object key, TItem value, DateTimeOffset absoluteExpiration)
        {
            throw new NotImplementedException();
        }

        public TItem Set<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        {
            throw new NotImplementedException();
        }
    }
}
