using System;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Serial.Core
{
    public class SerialFactory : ISerialFactory
    {
        private readonly IServiceProvider? _serviceProvider;

        public SerialFactory(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        public ISerialWorker CreateWorker(SerialPortOptions options)
        {
            return new SerialWorker(options);
        }

        public ISerialProtocol CreateProtocol(Type protocolType)
        {
            if (_serviceProvider != null)
            {
                var service = _serviceProvider.GetService(protocolType);
                if (service is ISerialProtocol protocol)
                    return protocol;
            }

            var instance = Activator.CreateInstance(protocolType);
            if (instance is ISerialProtocol protocolInstance)
                return protocolInstance;

            throw new InvalidOperationException($"Type {protocolType.FullName} does not implement ISerialProtocol.");
        }

        public IReconnectStrategy CreateReconnectStrategy(Type strategyType)
        {
            if (_serviceProvider != null)
            {
                var service = _serviceProvider.GetService(strategyType);
                if (service is IReconnectStrategy strategy)
                    return strategy;
            }

            var instance = Activator.CreateInstance(strategyType);
            if (instance is IReconnectStrategy strategyInstance)
                return strategyInstance;

            throw new InvalidOperationException($"Type {strategyType.FullName} does not implement IReconnectStrategy.");
        }
    }
}
