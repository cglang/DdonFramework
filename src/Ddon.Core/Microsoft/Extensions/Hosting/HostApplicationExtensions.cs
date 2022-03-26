using Ddon.Core;
using Microsoft.Extensions.Configuration;
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
    }
}
