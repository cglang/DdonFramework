using Ddon.AspNetCore.Filters;
using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Ddon.AspNetCore
{
    public class AspNetCoreModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            // 向容器当中添加一些功能组成WebApi，包含AddControllers、AddHttpContextAccessor、AppSrtings
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(GlobalExceptionFilter));
            });

            services.AddHttpContextAccessor();

            var cors = configuration.GetSection("Cors").GetChildren().Select(c => c.Value).ToArray();
            if (cors is not null)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(name: "DefaultCors", builder =>
                    {
                        builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(cors);
                    });
                });
            }
        }
    }
}
