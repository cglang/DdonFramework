using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Serial.Builder;

namespace Ddon.Serial.Abstractions
{
    public interface ISerialManager
    {
        ISerialEndpoint AddEndpoint(string name, System.Action<SerialEndpointBuilder> configure);

        bool RemoveEndpoint(string name);

        ISerialEndpoint? GetEndpoint(string name);

        IEnumerable<ISerialEndpoint> GetAllEndpoints();

        Task StartAllAsync(CancellationToken cancellationToken = default);

        Task StopAllAsync(CancellationToken cancellationToken = default);
    }
}
