using System;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Builder;
using Ddon.Serial.Core;
using Ddon.Serial.Hosted;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ddon.Serial.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSerial(
            this IServiceCollection services,
            Action<SerialBuilder> configure)
        {
            services.TryAddSingleton<ISerialManager>(sp =>
            {
                var manager = new SerialManager(sp, configure);
                return manager;
            });

            return services;
        }

        public static IServiceCollection AddSerialHostedService(this IServiceCollection services)
        {
            services.AddHostedService<SerialHostedService>();
            return services;
        }
    }
}
