using System;
using Ddon.OpenProtocol.Abstractions;

namespace Ddon.OpenProtocol.Builder
{
    public class OpenProtocolBuilder
    {
        private readonly IOpenProtocolManager _manager;

        internal OpenProtocolBuilder(IOpenProtocolManager manager)
        {
            _manager = manager;
        }

        public OpenProtocolBuilder AddEndpoint(string name, Action<OpenProtocolEndpointBuilder> configure)
        {
            _manager.AddEndpoint(name, configure);
            return this;
        }
    }
}
