using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Serial.Abstractions
{
    public interface ISerialWorker : IDisposable
    {
        string PortName { get; }

        bool IsOpen { get; }

        Task OpenAsync(CancellationToken cancellationToken = default);

        Task CloseAsync(CancellationToken cancellationToken = default);

        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

        event EventHandler<byte[]> DataReceived;

        event EventHandler<Exception> ErrorOccurred;
    }
}
