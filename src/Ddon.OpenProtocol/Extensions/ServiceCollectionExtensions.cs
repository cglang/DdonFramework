using System;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Builder;
using Ddon.OpenProtocol.Core;
using Ddon.OpenProtocol.Hosted;
using Ddon.Socket.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ddon.OpenProtocol.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenProtocol(
            this IServiceCollection services,
            Action<OpenProtocolBuilder> configure)
        {
            services.TryAddSingleton<IOpenProtocolManager>(sp =>
            {
                var socketFactory = sp.GetRequiredService<ISocketFactory>();
                var loggerFactory = sp.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();

                var manager = new OpenProtocolManager(
                    socketFactory, sp, loggerFactory);

                var builder = new OpenProtocolBuilder(
                    manager, socketFactory, sp);

                configure(builder);

                return manager;
            });

            return services;
        }

        public static IServiceCollection AddOpenProtocolHostedService(
            this IServiceCollection services)
        {
            services.AddHostedService<OpenProtocolHostedService>();
            return services;
        }

        public static IServiceCollection AddOpenProtocolManager(
            this IServiceCollection services)
        {
            services.TryAddSingleton<IOpenProtocolManager>(sp =>
            {
                var socketFactory = sp.GetRequiredService<ISocketFactory>();
                var loggerFactory = sp.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                return new OpenProtocolManager(socketFactory, sp, loggerFactory);
            });

            return services;
        }
    }
}
