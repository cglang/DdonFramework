using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.OpenProtocol.Core
{
    /// <summary>
    /// Open Protocol TCP 传输层，负责连接的建立与原始字节收发。
    /// 不依赖任何上层框架，仅使用 <see cref="TcpClient"/>。
    /// </summary>
    public sealed class OpenProtocolConnection : IDisposable
    {
        private const int DefaultBufferSize = 4096;

        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private CancellationTokenSource _receiveCts = new();
        private Task? _receiveTask;
        private bool _disposed;

        /// <summary>收到一段原始字节数据（可能是一帧的一部分，由上层切帧）。</summary>
        public event EventHandler<byte[]>? DataReceived;

        /// <summary>连接断开（正常关闭或异常）。</summary>
        public event EventHandler? Disconnected;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        /// <summary>
        /// 建立 TCP 连接并启动接收循环。
        /// </summary>
        public async Task ConnectAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken)
        {
            try { _receiveCts.Cancel(); } catch { }
            _receiveCts.Dispose();
            _receiveCts = new CancellationTokenSource();

            _tcpClient = new TcpClient { NoDelay = true };

            var connectTask = _tcpClient.ConnectAsync(host, port);
            var completed = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, cancellationToken));
            if (completed != connectTask)
                throw new TimeoutException($"连接到 {host}:{port} 超时（{timeoutMs}ms）。");
            await connectTask;

            _stream = _tcpClient.GetStream();
            _receiveTask = ReceiveLoopAsync(_receiveCts.Token);
        }

        public async Task<int> SendAsync(byte[] data, CancellationToken cancellationToken)
        {
            if (_stream == null)
                throw new InvalidOperationException("连接未建立。");

            await _stream.WriteAsync(data, 0, data.Length, cancellationToken);
            await _stream.FlushAsync(cancellationToken);
            return data.Length;
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            try { _receiveCts.Cancel(); } catch { }

            if (_receiveTask != null)
            {
                try { await _receiveTask; } catch { }
                _receiveTask = null;
            }

            try { _stream?.Dispose(); } catch { }
            try { _tcpClient?.Dispose(); } catch { }
            _stream = null;
            _tcpClient = null;
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[DefaultBufferSize];

            try
            {
                while (!cancellationToken.IsCancellationRequested && _stream != null)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0)
                        break;

                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    DataReceived?.Invoke(this, data);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
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

            try { _receiveCts.Cancel(); } catch { }
            _receiveCts.Dispose();
            try { _stream?.Dispose(); } catch { }
            try { _tcpClient?.Dispose(); } catch { }
        }
    }
}
