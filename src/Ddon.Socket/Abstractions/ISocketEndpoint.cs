using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Socket.Abstractions
{
    public interface ISocketEndpoint
    {
        string Name { get; }

        bool IsRunning { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
