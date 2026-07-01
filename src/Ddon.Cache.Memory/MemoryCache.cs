using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Cache.Memory
{
    public class MemoryCache : CacheBase
    {
        public MemoryCache(IDistributedCache cache) : base(cache)
        {
        }

        public MemoryCache(IDistributedCache cache, JsonSerializerOptions options) : base(cache, options)
        {
        }
    }
}
