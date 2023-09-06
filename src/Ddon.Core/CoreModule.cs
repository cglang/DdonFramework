using System;
using System.Runtime;
using Ddon.Core.Scripts;
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
        services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
        services.AddSingleton<ISnowflakeGenerator, SnowflakeGenerator>();
        services.AddSingleton<IIdGenerator, IdGenerator>();

        services.Configure<SequentialGuidGeneratorOptions>(configuration.GetSection(nameof(SequentialGuidGeneratorOptions)));
        services.Configure<SnowflakeGeneratorOptions>(configuration.GetSection(nameof(SnowflakeGeneratorOptions)));

        services.AddSingleton<IOSPlatformProvider, OSPlatformProvider>();

        services.AddSingleton(typeof(IPipelineInstanceProvider<>), typeof(ContainerPipelineInstanceProvider<>));
        services.AddSingleton(typeof(IPipelineRegistrar<>), typeof(PipelineRegistrar<>));
        services.AddSingleton(typeof(IGeneralCustomPipeline<>), typeof(GeneralCustomPipeline<>));

        services.AddMediatR(config => config.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.AddSingleton<Python>();
        services.Configure<ScriptsOptions>(configuration.GetSection("Scripts"));
    }
}
