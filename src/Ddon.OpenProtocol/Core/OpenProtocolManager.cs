using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Builder;

namespace Ddon.OpenProtocol.Core
{
    public class OpenProtocolManager : IOpenProtocolManager
    {
        private readonly ConcurrentDictionary<string, IOpenProtocolEndpoint> _endpoints = new();
        private int _disposed;

        public IOpenProtocolEndpoint AddEndpoint(string name, Action<OpenProtocolEndpointBuilder> configure)
        {
            if (_endpoints.ContainsKey(name))
                throw new InvalidOperationException($"Endpoint '{name}' 已存在。");

            var builder = new OpenProtocolEndpointBuilder(name);
            configure(builder);
            var endpoint = builder.Build();

            if (!_endpoints.TryAdd(name, endpoint))
            {
                endpoint.Dispose();
                throw new InvalidOperationException($"Endpoint '{name}' 已存在。");
            }

            return endpoint;
        }

        public bool RemoveEndpoint(string name)
        {
            if (_endpoints.TryRemove(name, out var endpoint))
            {
                endpoint.Dispose();
                return true;
            }

            return false;
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
                await endpoint.ConnectAsync(cancellationToken);
            }
        }

        public async Task StopAllAsync(CancellationToken cancellationToken = default)
        {
            foreach (var endpoint in _endpoints.Values)
            {
                await endpoint.DisconnectAsync(cancellationToken);
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            foreach (var endpoint in _endpoints.Values)
            {
                endpoint.Dispose();
            }

            _endpoints.Clear();
        }
    }
}
