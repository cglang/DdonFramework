using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Cache.Redis
{
    public class RedisCache : CacheBase
    {
        public RedisCache(IDistributedCache cache) : base(cache)
        {
        }

        public RedisCache(IDistributedCache cache, JsonSerializerOptions options) : base(cache, options)
        {
        }
    }
}
