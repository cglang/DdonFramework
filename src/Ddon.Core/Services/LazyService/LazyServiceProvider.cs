using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Ddon.Core.Services.LazyService
{
    public sealed class LazyServiceProvider : ILazyServiceProvider
    {
        private IDictionary<Type, object> CachedServices { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public LazyServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            CachedServices = new Dictionary<Type, object>();
        }

        public T LazyGetRequiredService<T>()
        {
            return (T)LazyGetRequiredService(typeof(T));
        }

        public object LazyGetRequiredService(Type serviceType)
        {
            return CachedServices.GetOrAdd(serviceType, () => ServiceProvider.GetRequiredService(serviceType));
        }

        public T LazyGetService<T>()
        {
            return (T)LazyGetService(typeof(T));
        }

        public object LazyGetService(Type serviceType)
        {
            return CachedServices!.GetOrAdd(serviceType, () => ServiceProvider.GetService(serviceType))!;
        }

        public T LazyGetService<T>(T defaultValue)
        {
            return (T)LazyGetService(typeof(T), defaultValue!);
        }

        public object LazyGetService(Type serviceType, object defaultValue)
        {
            return LazyGetService(serviceType);
        }

        public T LazyGetService<T>(Func<IServiceProvider, object> factory)
        {
            return (T)LazyGetService(typeof(T), factory);
        }

        public object LazyGetService(Type serviceType, Func<IServiceProvider, object> factory)
        {
            return CachedServices.GetOrAdd(serviceType, () => factory(ServiceProvider));
        }
    }
}
