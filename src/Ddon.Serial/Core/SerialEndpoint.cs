using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Configuration;
using Ddon.Serial.Models;
using Microsoft.Extensions.Logging;

namespace Ddon.Serial.Core
{
    public class SerialEndpoint : ISerialEndpoint, IDisposable
    {
        private readonly string _name;
        private readonly SerialPortOptions _options;
        private readonly ISerialWorker _worker;
        private readonly ISerialProtocol? _protocol;
        private readonly ISerialPipeline _pipeline;
        private readonly SerialDispatcher _dispatcher;
        private readonly IReconnectStrategy? _reconnectStrategy;
        private readonly ILogger? _logger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly List<byte> _receiveBuffer = new List<byte>();
        private Task? _receiveTask;
        private int _retryCount;
        private bool _disposed;

        public SerialEndpoint(
            string name,
            SerialPortOptions options,
            ISerialWorker worker,
            ISerialPipeline pipeline,
            SerialDispatcher dispatcher,
            ISerialProtocol? protocol = null,
            IReconnectStrategy? reconnectStrategy = null,
            ILogger<SerialEndpoint>? logger = null)
        {
            _name = name;
            _options = options;
            _worker = worker;
            _protocol = protocol;
            _pipeline = pipeline;
            _dispatcher = dispatcher;
            _reconnectStrategy = reconnectStrategy;
            _logger = logger;
        }

        public string Name => _name;

        public string PortName => _options.PortName;

        public bool IsRunning { get; private set; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning) return;

            _worker.DataReceived += OnDataReceived;
            _worker.ErrorOccurred += OnErrorOccurred;

            await OpenWithRetryAsync(cancellationToken);

            IsRunning = true;
            _logger?.LogInformation("Serial endpoint '{Name}' started on {PortName}", _name, PortName);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRunning) return;

            _worker.DataReceived -= OnDataReceived;
            _worker.ErrorOccurred -= OnErrorOccurred;

            _cts.Cancel();

            if (_receiveTask != null)
            {
                try { await _receiveTask; } catch { }
            }

            await _worker.CloseAsync(cancellationToken);

            IsRunning = false;
            _logger?.LogInformation("Serial endpoint '{Name}' stopped", _name);
        }

        private async Task OpenWithRetryAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _worker.OpenAsync(cancellationToken);
                    _retryCount = 0;
                    return;
                }
                catch (Exception ex)
                {
                    _retryCount++;
                    _logger?.LogWarning(ex, "Failed to open {PortName}, retry {Retry}", PortName, _retryCount);

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
            _logger?.LogError(ex, "Error on serial endpoint '{Name}'", _name);
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

                    var context = new SerialContext
                    {
                        PortName = PortName,
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
                _logger?.LogError(ex, "Error processing data on {PortName}", PortName);
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
