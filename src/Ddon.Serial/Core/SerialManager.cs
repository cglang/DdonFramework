using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Builder;

namespace Ddon.Serial.Core
{
    public class SerialManager : ISerialManager
    {
        private readonly ConcurrentDictionary<string, ISerialEndpoint> _endpoints = new ConcurrentDictionary<string, ISerialEndpoint>();

        private readonly Action<SerialBuilder>? _configureAction;

        private readonly IServiceProvider? _serviceProvider;

        public SerialManager()
        {
        }

        public SerialManager(Action<SerialBuilder> configureAction)
        {
            _configureAction = configureAction;
        }

        public SerialManager(IServiceProvider serviceProvider, Action<SerialBuilder>? configureAction = null)
        {
            _serviceProvider = serviceProvider;
            _configureAction = configureAction;
        }

        public ISerialEndpoint AddEndpoint(string name, Action<SerialEndpointBuilder> configure)
        {
            if (_endpoints.ContainsKey(name))
                throw new InvalidOperationException($"Endpoint '{name}' already exists.");

            var builder = new SerialEndpointBuilder(name, _serviceProvider);
            configure(builder);
            var endpoint = builder.Build();

            _endpoints[name] = endpoint;
            return endpoint;
        }

        public bool RemoveEndpoint(string name)
        {
            if (_endpoints.TryRemove(name, out var endpoint))
            {
                return true;
            }
            return false;
        }

        public ISerialEndpoint? GetEndpoint(string name)
        {
            _endpoints.TryGetValue(name, out var endpoint);
            return endpoint;
        }

        public IEnumerable<ISerialEndpoint> GetAllEndpoints()
        {
            return _endpoints.Values;
        }

        public async Task StartAllAsync(CancellationToken cancellationToken = default)
        {
            if (_configureAction != null)
            {
                var builder = new SerialBuilder(this, _serviceProvider);
                _configureAction(builder);
            }

            foreach (var endpoint in _endpoints.Values)
            {
                await endpoint.StartAsync(cancellationToken);
            }
        }

        public async Task StopAllAsync(CancellationToken cancellationToken = default)
        {
            foreach (var endpoint in _endpoints.Values)
            {
                await endpoint.StopAsync(cancellationToken);
            }
        }
    }
}
