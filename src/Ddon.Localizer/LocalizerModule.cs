using Ddon.Cache;
using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Ddon.Localizer
{
    public class LocalizerModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);
            Load<CacheModule>(services, configuration);

            var localizerOptions = configuration.GetSection(nameof(JsonLocalizerOptions)).Get<JsonLocalizerOptions>();
            services.AddOptions().Configure<JsonLocalizerOptions>(options =>
            {
                options.ResourcesPath = localizerOptions.ResourcesPath;
                options.CacheKeyPrefix = localizerOptions.CacheKeyPrefix;
            });

            services.AddSingleton<IStringLocalizer, JsonStringLocalizer>();
        }
    }
}
