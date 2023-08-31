﻿using System.Runtime;
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

        var sequentialGuidGeneratorOptions = configuration.GetSection(nameof(SequentialGuidGeneratorOptions)).Get<SequentialGuidGeneratorOptions>() ?? new();
        services.AddOptions().Configure<SequentialGuidGeneratorOptions>(options =>
            options.DefaultSequentialGuidType = sequentialGuidGeneratorOptions.GetDefaultSequentialGuidType());

        var snowflakeGeneratorOptions = configuration.GetSection(nameof(SnowflakeGeneratorOptions)).Get<SnowflakeGeneratorOptions>() ?? new();
        services.AddOptions().Configure<SnowflakeGeneratorOptions>(options =>
            options.WorkerId = snowflakeGeneratorOptions.GetDefaultWorkerId());

        services.AddSingleton<IOSPlatformProvider, OSPlatformProvider>();

        services.AddSingleton(typeof(IPipelineInstanceProvider<>), typeof(ContainerPipelineInstanceProvider<>));
        services.AddSingleton(typeof(IPipelineRegistrar<>), typeof(PipelineRegistrar<>));
        services.AddSingleton(typeof(IGeneralCustomPipeline<>), typeof(GeneralCustomPipeline<>));
    }
}
