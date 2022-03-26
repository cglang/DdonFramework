using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Ddon.Cache
{
    public class StackExchangeRedisCache : RedisCache, ICache
    {
        private readonly IDistributedCache _distributedCache;

        public StackExchangeRedisCache(IOptions<RedisCacheOptions> optionsAccessor, IDistributedCache distributedCache) : base(optionsAccessor)
        {
            _distributedCache = distributedCache;
        }

        public Task<bool> ContainsKeyAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<TItem?> GetAsync<TItem>(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string[] keys)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<TItem>(string key, TItem value)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<TItem>(string key, TItem value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
