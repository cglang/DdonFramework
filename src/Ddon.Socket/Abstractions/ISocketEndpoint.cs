using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Socket.Abstractions
{
    public interface ISocketEndpoint : IDisposable
    {
        string Name { get; }

        bool IsRunning { get; }

        ISocketWorker Worker { get; }

        event EventHandler? Disconnected;

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
