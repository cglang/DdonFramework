using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Ddon.VitrinPLC.Abstractions;

namespace Ddon.VitrinPLC
{
    public static class PlcMirrorServiceCollectionExtensions
    {
        public static IServiceCollection AddVitrinPlc(
            this IServiceCollection services,
            Action<VitrinPlcBuilder> configure)
        {
            var builder = new VitrinPlcBuilder();
            configure(builder);

            services.AddSingleton(sp =>
            {
                var sessions = new Dictionary<string, IPlcSession>();
                var engines = new Dictionary<string, IPlcSyncEngine>();

                foreach (var descriptor in builder.Descriptors)
                {
                    var client = descriptor.ClientFactory(sp);
                    var group = PlcServiceFactory.Build(client, descriptor.Options, sp);
                    sessions[descriptor.Name] = group.Session;
                    engines[descriptor.Name] = group.Engine;
                }

                return new PlcHub(sessions, engines, sp);
            });

            services.AddSingleton<IPlcHub>(sp => sp.GetRequiredService<PlcHub>());
            services.AddHostedService<VitrinPlcHostedService>();

            return services;
        }
    }
}

