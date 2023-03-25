using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ddon.Core.Use.Reflection;
using Microsoft.AspNetCore.Builder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class HostApplicationExtensions
{
    public static IServiceCollection LoadModule<TMoudle>(this IServiceCollection services,
        IConfiguration configuration
    )
        where TMoudle : Module, new()
    {
        var module = new TMoudle();
        if (ModuleCore.CacheModule(module))
        {
            module.Load(services, configuration);
        }

        return services;
    }

    public static IHostBuilder CreateApplication<TMoudle>(this IHostBuilder hostBuilder,
        Action<IServiceCollection> configureDelegate
    ) where TMoudle : Module, new()
    {
        hostBuilder.ConfigureServices((_, services) =>
        {
            configureDelegate(services);
            var configuration =
                (IConfiguration)services.Single(p => p.ServiceType == typeof(IConfiguration)).ImplementationInstance!;
            var module = new TMoudle();
            if (ModuleCore.CacheModule(module))
            {
                module.Load(services, configuration);
            }
        });
        return hostBuilder;
    }

    public static async Task InitApplicationHttpMiddlewareAsync(this IApplicationBuilder app,
        IHostEnvironment env)
    {
        foreach (var module in ModuleInfo.Instance.Modules)
        {
            var myMethod = module.GetType().GetMethod(nameof(ModuleCore.HttpMiddleware))!;
            await DdonInvoke.InvokeAsync(module, myMethod, app, env);
        }
        
        ModuleInfo.Instance.Dispose();
    }
}
