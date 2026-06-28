using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Socket.Abstractions
{
    public interface ISocketWorker : IDisposable
    {
        string ConnectionId { get; }

        bool IsConnected { get; }

        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task DisconnectAsync(CancellationToken cancellationToken = default);

        Task<int> SendAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

        event EventHandler<byte[]> DataReceived;

        event EventHandler<Exception> ErrorOccurred;

        event EventHandler Disconnected;
    }
}
