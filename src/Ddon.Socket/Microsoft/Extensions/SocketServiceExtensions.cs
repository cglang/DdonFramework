using System;
using Ddon.Socket.Hosting;
using Ddon.Socket.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class SocketServiceExtensions
{
    public static IServiceCollection AddSocketServerService(
        this IServiceCollection services,
        Action<SocketServerOption> options)
    {
        services.AddOptions().Configure(options);

        services.AddHostedService<SocketBackgroundService>();

        return services;
    }
}
