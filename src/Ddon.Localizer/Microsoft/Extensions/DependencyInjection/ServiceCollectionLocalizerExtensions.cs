using System;
using Ddon.Localizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionLocalizerExtensions
    {
        public static IServiceCollection AddJsonLocalizer(this IServiceCollection services)
        {
            services.AddOptions();
            services.AddSingleton<IStringLocalizer, JsonStringLocalizer>();
            return services;
        }

        public static IServiceCollection AddJsonLocalizer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<JsonLocalizerOptions>(
                configuration.GetSection(nameof(JsonLocalizerOptions)));
            services.AddSingleton<IStringLocalizer, JsonStringLocalizer>();
            return services;
        }

        public static IServiceCollection AddJsonLocalizer(
            this IServiceCollection services,
            Action<JsonLocalizerOptions> configure)
        {
            services.Configure(configure);
            services.AddSingleton<IStringLocalizer, JsonStringLocalizer>();
            return services;
        }
    }
}
