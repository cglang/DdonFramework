using System;
using Ddon.Cache.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 注册Redis缓存相关服务的扩展方法
    /// </summary>
    public static class ServiceCollectionRedisCacheExtensions
    {
        public static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                configuration
                    .GetSection(nameof(CacheRedisOptions))
                    .Bind(options);
            });
        }

        public static IServiceCollection AddRedisCache(
            this IServiceCollection services,
            Action<RedisCacheOptions> configure)
        {
            var options = new RedisCacheOptions();

            configure?.Invoke(options);

            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = options.Configuration;
                redisOptions.ConfigurationOptions = options.ConfigurationOptions;
                redisOptions.InstanceName = options.InstanceName;
            });

            return services;
        }
    }
}
