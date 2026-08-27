using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Builder;

namespace Ddon.OpenProtocol.Abstractions
{
    public interface IOpenProtocolManager : IDisposable
    {
        IOpenProtocolEndpoint AddEndpoint(string name, Action<OpenProtocolEndpointBuilder> configure);

        bool RemoveEndpoint(string name);

        IOpenProtocolEndpoint? GetEndpoint(string name);

        IEnumerable<IOpenProtocolEndpoint> GetAllEndpoints();

        Task StartAllAsync(CancellationToken cancellationToken = default);

        Task StopAllAsync(CancellationToken cancellationToken = default);
    }
}
