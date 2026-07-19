using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Builder;
using Ddon.Socket.Abstractions;
using Microsoft.Extensions.Logging;

namespace Ddon.OpenProtocol.Core
{
    public class OpenProtocolManager : IOpenProtocolManager
    {
        private readonly ConcurrentDictionary<string, IOpenProtocolEndpoint> _endpoints = new();
        private readonly ISocketFactory _socketFactory;
        private readonly IServiceProvider? _serviceProvider;
        private readonly ILoggerFactory? _loggerFactory;

        public OpenProtocolManager(
            ISocketFactory socketFactory,
            IServiceProvider? serviceProvider = null,
            ILoggerFactory? loggerFactory = null)
        {
            _socketFactory = socketFactory;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
        }

        public void AddEndpoint(string name, Action<OpenProtocolEndpointBuilder> configure)
        {
            if (_endpoints.ContainsKey(name))
                throw new InvalidOperationException($"Endpoint '{name}' already exists.");

            var builder = new OpenProtocolEndpointBuilder(name, _socketFactory, _serviceProvider, _loggerFactory);
            configure(builder);
            var endpoint = builder.Build();
            _endpoints[name] = endpoint;
        }

        public void AddEndpoint(string name, IOpenProtocolEndpoint endpoint)
        {
            if (!_endpoints.TryAdd(name, endpoint))
                throw new InvalidOperationException($"Endpoint '{name}' already exists.");
        }

        public bool RemoveEndpoint(string name)
        {
            return _endpoints.TryRemove(name, out _);
        }

        public IOpenProtocolEndpoint? GetEndpoint(string name)
        {
            _endpoints.TryGetValue(name, out var endpoint);
            return endpoint;
        }

        public IEnumerable<IOpenProtocolEndpoint> GetAllEndpoints()
        {
            return _endpoints.Values;
        }

        public async Task StartAllAsync(CancellationToken cancellationToken = default)
        {
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
