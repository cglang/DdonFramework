using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ddon.Cache
{
    public class CacheModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            var redisEnabled = configuration["Redis:IsEnabled"];
            if (!redisEnabled.IsNullOrWhiteSpace() && bool.Parse(redisEnabled))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    var redisConfiguration = configuration["Redis:Configuration"];
                    if (!redisConfiguration.IsNullOrEmpty())
                    {
                        options.Configuration = redisConfiguration;
                    }
                });

                // 替换之前的注入
                //services.Replace(ServiceDescriptor.Singleton<IDistributedCache, StackExchangeRedisCache>());

                services.AddSingleton<ICache, StackExchangeRedisCache>();
            }
            else
            {
                services.AddMemoryCache();
                services.AddSingleton<ICache, MemoryCache>();
            }
        }
    }
}
