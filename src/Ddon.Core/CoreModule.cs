using System;
using System.Runtime;
using Ddon.Core.Scripts;
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

        services.AddSingleton<IOSPlatformProvider, OSPlatformProvider>();

        services.AddSingleton(typeof(IPipelineInstanceProvider<>), typeof(ContainerPipelineInstanceProvider<>));
        services.AddSingleton(typeof(IPipelineRegistrar<>), typeof(PipelineRegistrar<>));
        services.AddSingleton(typeof(IGeneralCustomPipeline<>), typeof(GeneralCustomPipeline<>));

        services.AddMediatR(config => config.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.AddSingleton<Python>();
        services.Configure<ScriptsBinPathOptions>(configuration.GetSection("Scripts"));
    }
}
