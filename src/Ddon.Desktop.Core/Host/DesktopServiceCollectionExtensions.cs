using Ddon.Desktop.Core.Bridge;
using Ddon.Desktop.Core.Platform;
using Ddon.Desktop.Core.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Desktop.Core.Host;

public static class DesktopServiceCollectionExtensions
{
    public static IServiceCollection AddDesktop(this IServiceCollection services)
    {
        services.AddSingleton<IUiBridge, UiBridge>();
        services.AddSingleton<IBridgeDispatcher, BridgeDispatcher>();
        services.AddHttpClient();
        return services;
    }

    public static IServiceCollection AddDesktopTransport<T>(this IServiceCollection services)
        where T : class, ITransport
    {
        services.AddSingleton<ITransport, T>();
        return services;
    }

    public static IServiceCollection AddPlatformService<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class, IPlatformService
        where TImplementation : class, TInterface
    {
        services.AddSingleton<TInterface, TImplementation>();
        return services;
    }
}
