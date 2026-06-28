using System;
using System.Net.Sockets;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Socket.Core
{
    public class SocketFactory : ISocketFactory
    {
        private readonly IServiceProvider? _serviceProvider;

        public SocketFactory(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        public ISocketWorker CreateWorker(SocketClientOptions options)
        {
            return new SocketWorker(options);
        }

        public ISocketWorker CreateWorker(TcpClient acceptedClient, SocketClientOptions options)
        {
            return new SocketWorker(acceptedClient, options);
        }

        public ISocketProtocol CreateProtocol(Type protocolType)
        {
            if (_serviceProvider != null)
            {
                var service = _serviceProvider.GetService(protocolType);
                if (service is ISocketProtocol protocol)
                    return protocol;
            }

            var instance = Activator.CreateInstance(protocolType);
            if (instance is ISocketProtocol protocolInstance)
                return protocolInstance;

            throw new InvalidOperationException($"Type {protocolType.FullName} does not implement ISocketProtocol.");
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
