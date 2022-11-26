using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Cache
{
    public interface ICache : IDistributedCache
    {
        Task<TItem?> GetAsync<TItem>(string key, CancellationToken token = default);

        Task SetAsync<TItem>(string key, TItem value, CancellationToken token = default);

        Task SetAsync<TItem>(string key, TItem value, DistributedCacheEntryOptions options, CancellationToken token = default);

        Task<bool> ContainsKeyAsync(string key);

        Task RemoveAsync(string[] keys, CancellationToken token = default);
    }
}
