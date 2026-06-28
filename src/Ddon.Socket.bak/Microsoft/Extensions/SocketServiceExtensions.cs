using System;
using System.Net;
using Ddon.Socket.Hosting;
using Ddon.Socket.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class SocketServiceExtensions
{
    public static IServiceCollection AddSocketServerService(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SocketServerOption> options)
    {
        SetConfigure(services, configuration, options);

        services.AddHostedService<SocketBackgroundService>();

        return services;
    }

    private static void SetConfigure(
        IServiceCollection services,
        IConfiguration configuration,
        Action<SocketServerOption> options)
    {
        services.Configure(options);

        var address = configuration["SocketServer:Address"];
        if (address is not null)
            services.Configure<SocketServerOption>(opt => opt.IPEndPoint.Address = IPAddress.Parse(address));

        var port = configuration["SocketServer:Port"];
        if (port is not null)
            services.Configure<SocketServerOption>(opt => opt.IPEndPoint.Port = int.Parse(port));
    }
}
