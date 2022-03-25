using Ddon.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Ddon.Application
{
    public class ApplicationModule : Core.Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoInject();

            var appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddAutoMapper(Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(Assembly.Load));
        }
    }
}
