using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using Module = Ddon.Core.Module;

namespace Ddon.Application
{
    public class ApplicationModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);

            services.AddAutoMapper(Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(Assembly.Load));
        }
    }
}
