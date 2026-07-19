using System;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Builder;
using Ddon.OpenProtocol.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenProtocolManager(
            this IServiceCollection services)
        {
            services.TryAddSingleton<IOpenProtocolManager>(sp =>
            {
                var socketFactory = sp.GetRequiredService<Ddon.Socket.Abstractions.ISocketFactory>();
                var loggerFactory = sp.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                return new OpenProtocolManager(socketFactory, sp, loggerFactory);
            });

            return services;
        }
    }
}
