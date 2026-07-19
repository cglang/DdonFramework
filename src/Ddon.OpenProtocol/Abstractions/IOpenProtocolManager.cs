using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Builder;

namespace Ddon.OpenProtocol.Abstractions
{
    public interface IOpenProtocolManager
    {
        void AddEndpoint(string name, Action<OpenProtocolEndpointBuilder> configure);

        void AddEndpoint(string name, IOpenProtocolEndpoint endpoint);

        bool RemoveEndpoint(string name);

        IOpenProtocolEndpoint? GetEndpoint(string name);

        IEnumerable<IOpenProtocolEndpoint> GetAllEndpoints();

        Task StartAllAsync(CancellationToken cancellationToken = default);

        Task StopAllAsync(CancellationToken cancellationToken = default);
    }
}
