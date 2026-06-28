using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Serial.Abstractions
{
    public interface ISerialEndpoint
    {
        string Name { get; }

        string PortName { get; }

        bool IsRunning { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
