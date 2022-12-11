using Ddon.Core.Services.Guids;
using Ddon.Core.Services.LazyService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Core
{
    public class CoreModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoInject();

            services.AddTransient<ILazyServiceProvider, LazyServiceProvider>();
            services.AddTransient<IGuidGenerator, SequentialGuidGenerator>();
        }
    }
}
