using System;
using System.Collections.Generic;
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
            services.AddSingleton(configure);
            services.TryAddSingleton<ISocketManager>(sp =>
            {
                var actions = sp.GetServices<Action<SocketBuilder>>();
                var manager = new SocketManager(sp, actions);
                return manager;
            });

            return services;
        }

        public static IServiceCollection AddSocketHostedService(this IServiceCollection services)
        {
            services.TryAddSingleton<ISocketManager>(sp =>
            {
                var actions = sp.GetServices<Action<SocketBuilder>>();
                var manager = new SocketManager(sp, actions);
                return manager;
            });
            services.AddHostedService<SocketHostedService>();
            return services;
        }
    }
}
