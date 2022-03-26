using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Cache
{
    public class Cache : ICache
    {
        private readonly IDistributedCache _memoryCache;

        public Cache(IDistributedCache distributedCache)
        {
            _memoryCache = distributedCache;
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            return (await _memoryCache.GetAsync(key)) is not null;
        }

        public async Task<TItem?> GetAsync<TItem>(string key)
        {
            var bytes = await _memoryCache.GetAsync(key);
            if (bytes != null)
            {
                var json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<TItem>(json);
            }
            return default;
        }

        public async Task RemoveAsync(string key)
        {
            await _memoryCache.RemoveAsync(key);
        }

        public async Task RemoveAsync(string[] keys)
        {
            Parallel.ForEach(keys, async key => await RemoveAsync(key));
            await Task.CompletedTask;
        }

        public async Task SetAsync<TItem>(string key, TItem value)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await _memoryCache.SetAsync(key, bytes);
        }

        public async Task SetAsync<TItem>(string key, TItem value, DistributedCacheEntryOptions options)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await _memoryCache.SetAsync(key, bytes, options);
        }
    }
}
