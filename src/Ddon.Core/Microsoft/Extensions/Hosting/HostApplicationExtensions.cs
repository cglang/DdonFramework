using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Microsoft.Extensions.Hosting
{
    public static class HostApplicationExtensions
    {
        public static IHostBuilder CreateApplication<TMoudle>(this IHostBuilder hostBuilder)
            where TMoudle : Module, new()
        {
            hostBuilder.ConfigureServices((x, services) =>
            {
                var configuration = (IConfiguration)services.Single(p => p.ServiceType == typeof(IConfiguration)).ImplementationInstance!;
                new TMoudle().Load(services, configuration);
            });
            return hostBuilder;
        }

        public static IServiceCollection LoadModule<TMoudle>(this IServiceCollection services, IConfiguration configuration) where TMoudle : Module, new()
        {
            new TMoudle().Load(services, configuration);
            return services;
        }
    }
}
