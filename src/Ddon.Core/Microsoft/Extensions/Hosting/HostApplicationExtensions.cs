using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class HostApplicationExtensions
    {
        public static IServiceCollection LoadModule<TMoudle>(this IServiceCollection services, IConfiguration configuration) where TMoudle : Module, new()
        {
            new TMoudle().Load(services, configuration);
            return services;
        }
    }
}
