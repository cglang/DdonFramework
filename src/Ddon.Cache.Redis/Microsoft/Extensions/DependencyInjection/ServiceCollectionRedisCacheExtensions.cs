using System;
using Ddon.Cache;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using RedisCacheOptions = Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionRedisCacheExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                configuration
                    .GetSection("CacheRedisOptions")
                    .Bind(options);
            });

            services.AddSingleton<ICache, Ddon.Cache.Redis.RedisCache>();

            return services;
        }

        public static IServiceCollection AddRedisCache(
            this IServiceCollection services,
            Action<RedisCacheOptions> configure)
        {
            services.AddStackExchangeRedisCache(configure);
            services.AddSingleton<ICache, Ddon.Cache.Redis.RedisCache>();

            return services;
        }
    }
}
