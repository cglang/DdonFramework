using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Cache
{
    public abstract class CacheBase : ICache
    {
        private readonly IDistributedCache _cache;

        protected JsonSerializerOptions SerializerOptions { get; }

        protected CacheBase(IDistributedCache cache, JsonSerializerOptions options = null)
        {
            _cache = cache;
            SerializerOptions = options ?? new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };
        }

        public async Task<TItem> GetAsync<TItem>(string key, CancellationToken token = default)
        {
            var bytes = await _cache.GetAsync(key, token);
            if (bytes is null) return default;
            return JsonSerializer.Deserialize<TItem>(bytes, SerializerOptions);
        }

        public Task SetAsync<TItem>(string key, TItem value, CancellationToken token = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
            return _cache.SetAsync(key, bytes, token);
        }

        public Task SetAsync<TItem>(string key, TItem value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
            return _cache.SetAsync(key, bytes, options, token);
        }

        public async Task<bool> ContainsKeyAsync(string key, CancellationToken token = default)
        {
            return (await _cache.GetAsync(key, token)) != null;
        }

        public virtual Task RemoveAsync(string[] keys, CancellationToken token = default)
        {
            return Task.WhenAll(keys.Select(key => RemoveAsync(key, token)));
        }

        public byte[] Get(string key) => _cache.Get(key);

        public Task<byte[]> GetAsync(string key, CancellationToken token = default) => _cache.GetAsync(key, token);

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _cache.Set(key, value, options);

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => _cache.SetAsync(key, value, options, token);

        public void Refresh(string key) => _cache.Refresh(key);

        public Task RefreshAsync(string key, CancellationToken token = default) => _cache.RefreshAsync(key, token);

        public void Remove(string key) => _cache.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default) => _cache.RemoveAsync(key, token);
    }
}
