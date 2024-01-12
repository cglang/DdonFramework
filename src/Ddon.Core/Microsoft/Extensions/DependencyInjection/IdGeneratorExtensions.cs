using System;
using Ddon.Core.Services.IdWorker;
using Ddon.Core.Services.IdWorker.Guids;
using Ddon.Core.Services.IdWorker.Snowflake;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdGeneratorExtensions
    {
        public static void AddGuidGenerator(this IServiceCollection services, Action<SequentialGuidGeneratorOptions> options)
        {
            services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
            services.Configure(options);
        }

        public static void AddSnowflakeGenerator(this IServiceCollection services, Action<SnowflakeGeneratorOptions> options)
        {
            services.AddSingleton<ISnowflakeGenerator, SnowflakeGenerator>();
            services.Configure(options);
        }

        public static void AddIdGenerator(
            this IServiceCollection services,
            Action<SequentialGuidGeneratorOptions> sequentialOptions,
            Action<SnowflakeGeneratorOptions> snowflakeOptions)
        {
            AddGuidGenerator(services, sequentialOptions);
            AddSnowflakeGenerator(services, snowflakeOptions);

            services.AddSingleton<IIdGenerator, IdGenerator>();
        }
    }
}
