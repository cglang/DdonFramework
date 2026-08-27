using System;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Builder;
using Ddon.OpenProtocol.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ddon.OpenProtocol.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenProtocol(
            this IServiceCollection services,
            Action<OpenProtocolBuilder>? configure = null)
        {
            if (configure != null)
            {
                services.AddSingleton(configure);
            }

            services.TryAddSingleton<IOpenProtocolManager>(sp =>
            {
                var manager = new OpenProtocolManager();

                foreach (var action in sp.GetServices<Action<OpenProtocolBuilder>>())
                {
                    action(new OpenProtocolBuilder(manager));
                }

                return manager;
            });

            return services;
        }

        public static IServiceCollection AddOpenProtocolHostedService(this IServiceCollection services)
        {
            services.AddOpenProtocol();
            services.AddHostedService<Hosted.OpenProtocolHostedService>();
            return services;
        }
    }
}
