using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Cache.Memory
{
    public class Cache : ICache
    {
        private readonly IDistributedCache _distributedCache;

        public JsonSerializerOptions JsonSerializeroptions { get; set; }

        public Cache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;

            JsonSerializeroptions = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            return await _distributedCache.GetAsync(key) is not null;
        }

        public async Task<TItem?> GetAsync<TItem>(string key, CancellationToken token = default)
        {
            var bytes = await _distributedCache.GetAsync(key, token);
            if (bytes != null)
            {
                var json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<TItem>(json);
            }
            return default;
        }

        public Task RemoveAsync(string[] keys, CancellationToken token = default)
        {
            return Parallel.ForEachAsync(keys, async (key, token) => await RemoveAsync(key, token));
        }

        public Task SetAsync<TItem>(string key, TItem value, CancellationToken token = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializeroptions);
            return _distributedCache.SetAsync(key, bytes, token);
        }

        public Task SetAsync<TItem>(string key, TItem value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializeroptions);
            return _distributedCache.SetAsync(key, bytes, options, token);
        }

        public byte[]? Get(string key) => _distributedCache.Get(key);

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => _distributedCache.GetAsync(key, token);

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _distributedCache.Set(key, value, options);

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => _distributedCache.SetAsync(key, value, options);

        public void Refresh(string key) => _distributedCache.Refresh(key);

        public Task RefreshAsync(string key, CancellationToken token = default) => _distributedCache.RefreshAsync(key, token);

        public void Remove(string key) => _distributedCache.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default) => _distributedCache.RemoveAsync(key, token);
    }
}
