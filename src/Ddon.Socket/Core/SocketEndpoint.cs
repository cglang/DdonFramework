using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Models;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket.Core
{
    public class SocketEndpoint : ISocketEndpoint, IDisposable
    {
        private readonly string _name;
        private readonly ISocketWorker _worker;
        private readonly ISocketProtocol? _protocol;
        private readonly ISocketPipeline _pipeline;
        private readonly SocketDispatcher _dispatcher;
        private readonly IReconnectStrategy? _reconnectStrategy;
        private readonly ILogger? _logger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly List<byte> _receiveBuffer = new List<byte>();
        private readonly bool _isServerEndpoint;
        private Task? _receiveTask;
        private Task? _reconnectTask;
        private int _retryCount;
        private bool _disposed;

        public SocketEndpoint(
            string name,
            ISocketWorker worker,
            ISocketPipeline pipeline,
            SocketDispatcher dispatcher,
            ISocketProtocol? protocol = null,
            IReconnectStrategy? reconnectStrategy = null,
            ILogger<SocketEndpoint>? logger = null,
            bool isServerEndpoint = false)
        {
            _name = name;
            _worker = worker;
            _protocol = protocol;
            _pipeline = pipeline;
            _dispatcher = dispatcher;
            _reconnectStrategy = reconnectStrategy;
            _logger = logger;
            _isServerEndpoint = isServerEndpoint;
        }

        public string Name => _name;

        public bool IsRunning { get; private set; }

        public ISocketWorker Worker => _worker;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning) return;

            _worker.DataReceived += OnDataReceived;
            _worker.ErrorOccurred += OnErrorOccurred;
            _worker.Disconnected += OnDisconnected;

            await ConnectWithRetryAsync(cancellationToken);

            IsRunning = true;
            _logger?.LogInformation("Socket endpoint '{Name}' connected", _name);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRunning) return;

            _cts.Cancel();

            if (_reconnectTask != null)
            {
                try { await _reconnectTask; } catch { }
            }

            _worker.DataReceived -= OnDataReceived;
            _worker.ErrorOccurred -= OnErrorOccurred;
            _worker.Disconnected -= OnDisconnected;

            if (_receiveTask != null)
            {
                try { await _receiveTask; } catch { }
            }

            await _worker.DisconnectAsync(cancellationToken);

            IsRunning = false;
            _logger?.LogInformation("Socket endpoint '{Name}' stopped", _name);
        }

        private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _worker.ConnectAsync(cancellationToken);
                    _retryCount = 0;
                    return;
                }
                catch (Exception ex)
                {
                    _retryCount++;
                    _logger?.LogWarning(ex, "Failed to connect, retry {Retry}", _retryCount);

                    if (_reconnectStrategy == null)
                        throw;

                    var delay = _reconnectStrategy.GetNextDelay(_retryCount);
                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                }
            }
        }

        private void OnDataReceived(object? sender, byte[] data)
        {
            _receiveTask = ProcessDataAsync(data, _cts.Token);
        }

        private void OnErrorOccurred(object? sender, Exception ex)
        {
            _logger?.LogError(ex, "Error on socket endpoint '{Name}'", _name);
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            IsRunning = false;
            _logger?.LogWarning("Socket endpoint '{Name}' disconnected", _name);

            if (!_isServerEndpoint && _reconnectStrategy != null && !_disposed)
            {
                _reconnectTask = ReconnectLoopAsync(_cts.Token);
            }
        }

        private async Task ReconnectLoopAsync(CancellationToken cancellationToken)
        {
            _retryCount = 0;

            try
            {
                await ConnectWithRetryAsync(cancellationToken);

                IsRunning = true;
                _logger?.LogInformation("Socket endpoint '{Name}' reconnected", _name);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Reconnect failed for endpoint '{Name}'", _name);
            }
        }

        private async Task ProcessDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            try
            {
                _receiveBuffer.AddRange(data);

                var buffer = _receiveBuffer.ToArray();
                int offset = 0;

                while (offset < buffer.Length)
                {
                    byte[]? frame;
                    int consumed;

                    if (_protocol != null)
                    {
                        (frame, consumed) = _protocol.Decode(buffer, offset, buffer.Length - offset);
                    }
                    else
                    {
                        frame = new byte[buffer.Length - offset];
                        Array.Copy(buffer, offset, frame, 0, frame.Length);
                        consumed = buffer.Length - offset;
                    }

                    if (frame == null)
                    {
                        _receiveBuffer.Clear();
                        var remaining = buffer.Length - offset;
                        if (remaining > 0)
                        {
                            var remainBytes = new byte[remaining];
                            Array.Copy(buffer, offset, remainBytes, 0, remaining);
                            _receiveBuffer.AddRange(remainBytes);
                        }
                        break;
                    }

                    var context = new SocketContext
                    {
                        Buffer = frame,
                        Length = frame.Length,
                        ReceiveTime = DateTime.UtcNow,
                    };

                    await _pipeline.ExecuteAsync(context);
                    await _dispatcher.DispatchAsync(context, cancellationToken);

                    offset += consumed;
                }

                if (offset >= buffer.Length)
                {
                    _receiveBuffer.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing data");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts.Cancel();
            _cts.Dispose();
            _worker.Dispose();
        }
    }
}
