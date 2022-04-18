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

            var localizerOptions = new JsonLocalizerOptions();
            configuration.GetSection(nameof(localizerOptions)).Bind(localizerOptions);
            services.Configure<JsonLocalizerOptions>(options => options = localizerOptions);

            services.AddSingleton<IStringLocalizer, JsonStringLocalizer>();
        }
    }
}
