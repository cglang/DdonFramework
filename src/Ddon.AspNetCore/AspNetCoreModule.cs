using System;
using System.Linq;
using Ddon.AspNetCore.Filters;
using Ddon.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.AspNetCore
{
    public class AspNetCoreModule<TKey> : Module
        where TKey : IEquatable<TKey>
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.Insert(0, ServiceDescriptor.Singleton(typeof(IApplicationBuilder)));

            services.AddScoped<UserInfoInitMiddleware<TKey>>();

            // 向容器当中添加一些功能组成WebApi，包含AddControllers、AddHttpContextAccessor、AppSrtings
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(GlobalResultFilter));
            });

            services.AddHttpContextAccessor();

            var cors = configuration.GetSection("Cors").GetChildren().Select(c => c.Value ?? string.Empty).ToArray();
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

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            app.UseUserInfoInit<TKey>();
        }
    }
}
