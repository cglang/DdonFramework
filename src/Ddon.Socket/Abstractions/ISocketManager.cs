using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Builder;
using Ddon.Socket.Configuration;

namespace Ddon.Socket.Abstractions
{
    public interface ISocketManager
    {
        ISocketEndpoint AddEndpoint(string name, Action<SocketEndpointBuilder> configure);

        ISocketEndpoint AddEndpoint(string name, ISocketEndpoint endpoint);

        bool RemoveEndpoint(string name);

        ISocketEndpoint? GetEndpoint(string name);

        IEnumerable<ISocketEndpoint> GetAllEndpoints();

        void UseServer(string name, Action<SocketServerOptions> configureOptions, Action<SocketEndpointBuilder> configureEndpoint);

        Task StartAllAsync(CancellationToken cancellationToken = default);

        Task StopAllAsync(CancellationToken cancellationToken = default);
    }
}
