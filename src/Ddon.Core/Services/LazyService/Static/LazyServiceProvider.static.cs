using System;

namespace Ddon.Core.Services.LazyService.Static
{
    public class LazyServiceProvider
    {
        private static readonly Lazy<LazyServiceProvider> instanceHolder = new(() => new LazyServiceProvider());

        private ILazyServiceProvider? serviceProvider;

        private LazyServiceProvider() { }

        public static void InitServiceProvider(IServiceProvider serviceProvider)
        {
            instanceHolder.Value.serviceProvider = new LazyService.LazyServiceProvider(serviceProvider);
        }

        private static ILazyServiceProvider GetLazyServiceProvider()
        {
            if (instanceHolder.Value.serviceProvider is null)
                throw new InvalidOperationException("ServiceProvider 未初始化");
            return instanceHolder.Value.serviceProvider;
        }

        public static ILazyServiceProvider LazyServicePrivider => GetLazyServiceProvider();
    }
}
