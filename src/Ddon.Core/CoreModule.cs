using System;
using System.Runtime;
using Ddon.Core.Scripts;
using Ddon.Core.Services.LazyService;
using Ddon.SimpleModule;
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

        services.AddBasePipeline();

        services.AddMediatR(config => config.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.AddSingleton<Python>();
        services.Configure<ScriptsBinPathOptions>(configuration.GetSection("Scripts"));
    }
}
