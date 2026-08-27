using System;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Configuration;
using Ddon.OpenProtocol.Core;

namespace Ddon.OpenProtocol.Builder
{
    public class OpenProtocolEndpointBuilder
    {
        private readonly string _name;
        private readonly OpenProtocolClientOptions _options = new();

        internal OpenProtocolEndpointBuilder(string name)
        {
            _name = name;
            _options.Name = name;
        }

        public OpenProtocolEndpointBuilder Configure(Action<OpenProtocolClientOptions> configure)
        {
            configure(_options);
            return this;
        }

        internal IOpenProtocolEndpoint Build()
        {
            return new OpenProtocolEndpoint(_name, _options);
        }
    }
}
