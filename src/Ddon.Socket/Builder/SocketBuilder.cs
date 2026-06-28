using System;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Configuration;

namespace Ddon.Socket.Builder
{
    public class SocketBuilder
    {
        private readonly ISocketManager _manager;
        private readonly IServiceProvider? _serviceProvider;

        internal SocketBuilder(ISocketManager manager, IServiceProvider? serviceProvider = null)
        {
            _manager = manager;
            _serviceProvider = serviceProvider;
        }

        public ISocketEndpoint AddEndpoint(string name, Action<SocketEndpointBuilder> configure)
        {
            return _manager.AddEndpoint(name, configure);
        }

        public void UseServer(string name, Action<SocketServerOptions> configureOptions, Action<SocketEndpointBuilder> configureEndpoint)
        {
            _manager.UseServer(name, configureOptions, configureEndpoint);
        }
    }
}
