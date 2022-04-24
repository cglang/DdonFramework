using System;

namespace Ddon.Core
{
    public class DdonServiceProvider
    {
        private static readonly Lazy<DdonServiceProvider> instanceHolder = new(() => new DdonServiceProvider());

        private IServiceProvider? serviceProvider;

        private DdonServiceProvider() { }

        public static void InitServiceProvider(IServiceProvider serviceProvider)
        {
            instanceHolder.Value.serviceProvider = serviceProvider;
        }

        public static IServiceProvider GetServiceProvider()
        {
            if (instanceHolder.Value.serviceProvider is null)
                throw new InvalidOperationException("ServiceProvider 未初始化");
            return instanceHolder.Value.serviceProvider;
        }

        public static IServiceProvider Services => GetServiceProvider();
    }
}
