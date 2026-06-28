using System;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Builder;
using Ddon.Socket.Core;
using Ddon.Socket.Hosted;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ddon.Socket.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSocket(
            this IServiceCollection services,
            Action<SocketBuilder> configure)
        {
            services.TryAddSingleton<ISocketManager>(sp =>
            {
                var manager = new SocketManager(sp, configure);
                return manager;
            });

            return services;
        }

        public static IServiceCollection AddSocketHostedService(this IServiceCollection services)
        {
            services.AddHostedService<SocketHostedService>();
            return services;
        }
    }
}
