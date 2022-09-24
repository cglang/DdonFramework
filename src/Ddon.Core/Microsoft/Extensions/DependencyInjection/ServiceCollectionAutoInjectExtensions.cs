using Ddon.Core.Use.Di;
using System;
using System.Linq;

// ReSharper disable once CheckNamespace
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

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();
            implementTypes.ForEach(implementType =>
            {
                var interfaceType = types.FirstOrDefault(type => type.IsInterface && type.IsAssignableFrom(implementType));
                if (interfaceType is not null)
                    services.AddTransient(interfaceType, implementType);
                else
                    services.AddTransient(implementType);
            });
        }

        private static void InjectScoped(IServiceCollection services)
        {
            var baseType = typeof(IScopedDependency);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();
            implementTypes.ForEach(implementType =>
            {
                var interfaceType = types.FirstOrDefault(type => type.IsInterface && type.IsAssignableFrom(implementType));
                if (interfaceType is not null)
                    services.AddScoped(interfaceType, implementType);
                else
                    services.AddScoped(implementType);
            });
        }

        private static void InjectSingleton(IServiceCollection services)
        {
            var baseType = typeof(ISingletonDependency);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();
            implementTypes.ForEach(implementType =>
            {
                var interfaceType = types.FirstOrDefault(type => type.IsInterface && type.IsAssignableFrom(implementType));
                if (interfaceType is not null)
                    services.AddSingleton(interfaceType, implementType);
                else
                    services.AddSingleton(implementType);
            });
        }
    }
}
