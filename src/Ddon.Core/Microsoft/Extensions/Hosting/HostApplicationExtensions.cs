using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Microsoft.Extensions.Hosting
{
    public static class HostApplicationExtensions
    {
        public static IServiceCollection LoadModule<TMoudle>(this IServiceCollection services, IConfiguration configuration) where TMoudle : Module, new()
        {
            new TMoudle().Load(services, configuration);
            return services;
        }

        public static IHostBuilder CreateApplication<TMoudle>(this IHostBuilder hostBuilder, Action<IServiceCollection> configureDelegate) where TMoudle : Module, new()
        {
            hostBuilder.ConfigureServices((x, services) =>
            {
                configureDelegate(services);
                var configuration = (IConfiguration)services.Single(p => p.ServiceType == typeof(IConfiguration)).ImplementationInstance!;
                new TMoudle().Load(services, configuration);
            });
            return hostBuilder;
        }
    }
}
