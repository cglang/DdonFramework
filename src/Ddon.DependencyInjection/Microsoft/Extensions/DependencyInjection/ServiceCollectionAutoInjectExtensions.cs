using Ddon.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 自动批量注入
    /// </summary>
    public static class ServiceCollectionAutoInjectExtensions
    {
        public static void AddAutoInject(this IServiceCollection services)
        {
            InjectTransient(services);
            InjectScoped(services);
            InjectSingleton(services);
        }

        private static void InjectTransient(IServiceCollection services)
        {
            var baseType = typeof(ITransientDependency);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeGetTypes)
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();
            implementTypes.ForEach(implementType =>
            {
                var interfaceType = types.FirstOrDefault(type => type.IsInterface && type.IsAssignableFrom(implementType));
                if (interfaceType != null)
                    services.AddTransient(interfaceType, implementType);
                else
                    services.AddTransient(implementType);
            });
        }

        private static void InjectScoped(IServiceCollection services)
        {
            var baseType = typeof(IScopedDependency);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeGetTypes)
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();
            implementTypes.ForEach(implementType =>
            {
                var interfaceType = types.FirstOrDefault(type => type.IsInterface && type.IsAssignableFrom(implementType));
                if (interfaceType != null)
                    services.AddScoped(interfaceType, implementType);
                else
                    services.AddScoped(implementType);
            });
        }

        private static void InjectSingleton(IServiceCollection services)
        {
            var baseType = typeof(ISingletonDependency);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeGetTypes)
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();
            implementTypes.ForEach(implementType =>
            {
                var interfaceType = types.FirstOrDefault(type => type.IsInterface && type.IsAssignableFrom(implementType));
                if (interfaceType != null)
                    services.AddSingleton(interfaceType, implementType);
                else
                    services.AddSingleton(implementType);
            });
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // ex.Types 里包含已加载成功的类型
                return ex.Types.Where(t => t != null);
            }
        }
    }
}
