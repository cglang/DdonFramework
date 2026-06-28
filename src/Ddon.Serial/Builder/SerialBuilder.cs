using System;
using Ddon.Serial.Abstractions;

namespace Ddon.Serial.Builder
{
    public class SerialBuilder
    {
        private readonly ISerialManager _manager;
        private readonly IServiceProvider? _serviceProvider;

        internal SerialBuilder(ISerialManager manager, IServiceProvider? serviceProvider = null)
        {
            _manager = manager;
            _serviceProvider = serviceProvider;
        }

        public ISerialEndpoint AddEndpoint(string name, Action<SerialEndpointBuilder> configure)
        {
            return _manager.AddEndpoint(name, configure);
        }
    }
}
