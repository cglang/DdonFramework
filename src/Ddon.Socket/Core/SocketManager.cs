using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Builder;
using Ddon.Socket.Configuration;

namespace Ddon.Socket.Core
{
    public class SocketManager : ISocketManager
    {
        private readonly ConcurrentDictionary<string, ISocketEndpoint> _endpoints = new ConcurrentDictionary<string, ISocketEndpoint>();

        private readonly Action<SocketBuilder>? _configureAction;

        private readonly IServiceProvider? _serviceProvider;

        private SocketServer? _server;

        public SocketManager()
        {
        }

        public SocketManager(Action<SocketBuilder> configureAction)
        {
            _configureAction = configureAction;
        }

        public SocketManager(IServiceProvider serviceProvider, Action<SocketBuilder>? configureAction = null)
        {
            _serviceProvider = serviceProvider;
            _configureAction = configureAction;
        }

        public ISocketEndpoint AddEndpoint(string name, Action<SocketEndpointBuilder> configure)
        {
            if (_endpoints.ContainsKey(name))
                throw new InvalidOperationException($"Endpoint '{name}' already exists.");

            var builder = new SocketEndpointBuilder(name, _serviceProvider);
            configure(builder);
            var endpoint = builder.Build();

            _endpoints[name] = endpoint;
            return endpoint;
        }

        public ISocketEndpoint AddEndpoint(string name, ISocketEndpoint endpoint)
        {
            if (!_endpoints.TryAdd(name, endpoint))
                throw new InvalidOperationException($"Endpoint '{name}' already exists.");
            return endpoint;
        }

        public bool RemoveEndpoint(string name)
        {
            return _endpoints.TryRemove(name, out _);
        }

        public ISocketEndpoint? GetEndpoint(string name)
        {
            _endpoints.TryGetValue(name, out var endpoint);
            return endpoint;
        }

        public IEnumerable<ISocketEndpoint> GetAllEndpoints()
        {
            return _endpoints.Values;
        }

        public void UseServer(string name, Action<SocketServerOptions> configureOptions, Action<SocketEndpointBuilder> configureEndpoint)
        {
            var options = new SocketServerOptions();
            configureOptions(options);

            _server = new SocketServer(name, options, configureEndpoint, this, _serviceProvider);
        }

        public async Task StartAllAsync(CancellationToken cancellationToken = default)
        {
            if (_configureAction != null)
            {
                var builder = new SocketBuilder(this, _serviceProvider);
                _configureAction(builder);
            }

            if (_server != null)
            {
                await _server.StartAsync(cancellationToken);
            }

            foreach (var endpoint in _endpoints.Values)
            {
                await endpoint.StartAsync(cancellationToken);
            }
        }

        public async Task StopAllAsync(CancellationToken cancellationToken = default)
        {
            if (_server != null)
            {
                await _server.StopAsync();
            }

            foreach (var endpoint in _endpoints.Values)
            {
                await endpoint.StopAsync(cancellationToken);
            }
        }
    }
}
