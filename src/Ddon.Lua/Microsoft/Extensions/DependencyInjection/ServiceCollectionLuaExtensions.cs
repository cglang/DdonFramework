using System;
using Ddon.LuaEngine;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionLuaExtensions
    {
        public static IServiceCollection AddLua(this IServiceCollection services, Action<LuaOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }
            else
            {
                services.Configure<LuaOptions>(options => { });
            }

            services.AddSingleton<ILuaVmManager, LuaVmManager>();
            services.AddSingleton<ILuaScriptManager, LuaScriptManager>();

            return services;
        }
    }
}
