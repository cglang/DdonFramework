using System;
using Ddon.Cache;
using Ddon.Cache.Sqlite;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionSqliteCacheExtensions
    {
        public static IServiceCollection AddSqliteCache(
            this IServiceCollection services,
            Action<SqliteCacheOptions> configure)
        {
            var options = new SqliteCacheOptions();
            configure?.Invoke(options);
            services.AddSingleton<ICache>(new SqliteCache(options));
            return services;
        }
    }
}
