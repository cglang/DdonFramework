using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace Ddon.Cache
{
    public interface ICache
    {
        Task<TItem?> GetAsync<TItem>(string key);

        Task SetAsync<TItem>(string key, TItem value);

        Task SetAsync<TItem>(string key, TItem value, DistributedCacheEntryOptions options);

        Task<bool> ContainsKeyAsync(string key);

        Task RemoveAsync(string key);

        Task RemoveAsync(string[] keys);
    }
}
