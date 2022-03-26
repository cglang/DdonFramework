using DdonSocket;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DdonSocketExceptions
    {
        public static void AddDdonSocket<TDdonSocketHandler>(this IServiceCollection services, IConfiguration configuration) where TDdonSocketHandler : DdonSocketHandlerCore
        {
            services.AddSingleton<DdonSocketHandlerCore, TDdonSocketHandler>();
        }
    }
}
