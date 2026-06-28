using System;
using Ddon.OpenProtocol.Abstractions;
using Ddon.Socket.Abstractions;

namespace Ddon.OpenProtocol.Builder
{
    public class OpenProtocolBuilder
    {
        private readonly IOpenProtocolManager _manager;
        private readonly ISocketFactory _socketFactory;
        private readonly IServiceProvider? _serviceProvider;

        internal OpenProtocolBuilder(
            IOpenProtocolManager manager,
            ISocketFactory socketFactory,
            IServiceProvider? serviceProvider = null)
        {
            _manager = manager;
            _socketFactory = socketFactory;
            _serviceProvider = serviceProvider;
        }

        public IOpenProtocolEndpoint AddEndpoint(
            string name,
            Action<OpenProtocolEndpointBuilder> configure)
        {
            return _manager.AddEndpoint(name, configure);
        }
    }
}
