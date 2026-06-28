using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Configuration;

namespace Ddon.Socket.Core
{
    public class SocketWorker : ISocketWorker
    {
        private readonly SocketClientOptions? _options;
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private readonly CancellationTokenSource _receiveCts = new CancellationTokenSource();
        private Task? _receiveTask;
        private bool _disposed;
        private bool _isAccepted;

        public SocketWorker(SocketClientOptions options)
        {
            _options = options;
            ConnectionId = Guid.NewGuid().ToString("N");
        }

        public SocketWorker(TcpClient tcpClient, SocketClientOptions options)
        {
            _tcpClient = tcpClient;
            _options = options;
            _isAccepted = true;

            tcpClient.NoDelay = options.NoDelay;
            tcpClient.ReceiveBufferSize = options.ReceiveBufferSize;
            tcpClient.SendBufferSize = options.SendBufferSize;

            _stream = tcpClient.GetStream();
            ConnectionId = Guid.NewGuid().ToString("N");
            _receiveTask = ReceiveLoopAsync(_receiveCts.Token);
        }

        public string ConnectionId { get; }

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public event EventHandler<byte[]>? DataReceived;

        public event EventHandler<Exception>? ErrorOccurred;

        public event EventHandler? Disconnected;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_isAccepted) return;

            _tcpClient = new TcpClient();
            _tcpClient.NoDelay = _options!.NoDelay;
            _tcpClient.ReceiveBufferSize = _options.ReceiveBufferSize;
            _tcpClient.SendBufferSize = _options.SendBufferSize;

            using var timeoutCts = new CancellationTokenSource(_options.ConnectTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await _tcpClient.ConnectAsync(_options.Host, _options.Port);
                _stream = _tcpClient.GetStream();
                _receiveTask = ReceiveLoopAsync(_receiveCts.Token);
            }
            catch
            {
                _tcpClient.Dispose();
                _tcpClient = null;
                throw;
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            _receiveCts.Cancel();

            if (_receiveTask != null)
            {
                try { await _receiveTask; } catch { }
            }

            _stream?.Dispose();
            _stream = null;
            _tcpClient?.Dispose();
            _tcpClient = null;
        }

        public async Task<int> SendAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (_stream == null) throw new InvalidOperationException("Not connected");

            await _stream.WriteAsync(buffer, offset, count, cancellationToken);
            await _stream.FlushAsync(cancellationToken);
            return count;
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[_options?.ReceiveBufferSize ?? 4096];

            try
            {
                while (!cancellationToken.IsCancellationRequested && _stream != null)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead == 0) break;

                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    DataReceived?.Invoke(this, data);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
            finally
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _receiveCts.Cancel();
            _receiveCts.Dispose();
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }
    }
}
