using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Ddon.Plugin;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection LoadPlugins(this IServiceCollection services, string folderPath)
        {
            string assemblyDir = AppDomain.CurrentDomain.BaseDirectory;
            string pluginsDir = Path.Combine(assemblyDir, folderPath);

            if (!Directory.Exists(pluginsDir))
                Directory.CreateDirectory(pluginsDir);

            string[] dirs = Directory.GetDirectories(pluginsDir);

            foreach (var dir in dirs)
            {
                foreach (var dll in Directory.GetFiles(dir, "*.dll"))
                {
                    var assembly = Assembly.LoadFrom(dll);

                    // 注册插件
                    var types = assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var type in types)
                    {
                        services.AddTransient(typeof(IPlugin), type);
                    }
                }
            }

            return services;
        }
    }
}
