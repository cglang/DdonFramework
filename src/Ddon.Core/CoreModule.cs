using System.Runtime;
using Ddon.Core.Services.IdWorker;
using Ddon.Core.Services.IdWorker.Guids;
using Ddon.Core.Services.IdWorker.Snowflake;
using Ddon.Core.Services.LazyService;
using Ddon.Core.Use.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Core;

public class CoreModule : Module
{
    public override void Load(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoInject();

        services.AddTransient<ILazyServiceProvider, LazyServiceProvider>();

        var sequentialGuidGeneratorOptions = configuration.GetSection(nameof(SequentialGuidGeneratorOptions)).Get<SequentialGuidGeneratorOptions>() ?? new();
        services.AddOptions().Configure<SequentialGuidGeneratorOptions>(options =>
            options.DefaultSequentialGuidType = sequentialGuidGeneratorOptions.GetDefaultSequentialGuidType());
        services.AddTransient<IGuidGenerator, SequentialGuidGenerator>();

        var snowflakeGeneratorOptions = configuration.GetSection(nameof(SnowflakeGeneratorOptions)).Get<SnowflakeGeneratorOptions>() ?? new();
        services.AddOptions().Configure<SnowflakeGeneratorOptions>(options =>
            options.WorkerId = snowflakeGeneratorOptions.GetDefaultWorkerId());
        services.AddTransient<ISnowflakeGenerator, SnowflakeGenerator>();

        services.AddTransient<IIdGenerator, IdGenerator>();

        services.AddSingleton<IOSPlatformProvider, OSPlatformProvider>();

        services.AddSingleton(typeof(IMiddlewareInstanceProvider<>), typeof(ContainerMiddlewareInstanceProvider<>));
        services.AddSingleton(typeof(IMiddlewarePipelineRegistrar<>), typeof(MiddlewarePipelineRegistrar<>));
        services.AddSingleton(typeof(IGeneralCustomPipeline<>), typeof(GeneralCustomPipeline<>));
    }
}
