using Ddon.Core.Services.Guids;
using Ddon.Core.Use.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Core
{
    public class CoreModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoInject();

            services.Configure<SequentialGuidGeneratorOptions>(options =>
            {
                options.DefaultSequentialGuidType = SequentialGuidType.SequentialAsString;
            });

            //services.AddTransient<IDdonServiceInvoke, DdonServiceInvoke>();
        }
    }
}
