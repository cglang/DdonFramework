using System;

namespace Ddon.Core
{
    public class ServiceProviderFactory
    {
        private IServiceProvider? serviceProvider;

        private static ServiceProviderFactory? serviceProviderFactory;

        private static readonly object _lock = new();

        private ServiceProviderFactory() { }

        private static ServiceProviderFactory InitInstance()
        {
            if (serviceProviderFactory is not null) return serviceProviderFactory;
            lock (_lock)
            {
                serviceProviderFactory ??= new ServiceProviderFactory();
            }
            return serviceProviderFactory;
        }

        public static void InitServiceProvider(IServiceProvider serviceProvider)
        {
            var obj = InitInstance();
            obj.serviceProvider = serviceProvider;
        }

        public static IServiceProvider GetServiceProvider()
        {
            var obj = InitInstance();
            if (obj.serviceProvider is null)
                throw new InvalidOperationException("ServiceProvider 未初始化");
            return obj.serviceProvider;
        }
    }
}
