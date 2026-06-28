using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Configuration;

namespace Ddon.Serial.Core
{
    public class SerialWorker : ISerialWorker
    {
        private readonly SerialPort _serialPort;
        private readonly CancellationTokenSource _receiveCts = new CancellationTokenSource();
        private Task? _receiveTask;
        private bool _disposed;

        public SerialWorker(SerialPortOptions options)
        {
            PortName = options.PortName;

            _serialPort = new SerialPort(options.PortName, options.BaudRate, options.Parity, options.DataBits, options.StopBits)
            {
                Handshake = options.Handshake,
                ReadTimeout = options.ReadTimeout,
                WriteTimeout = options.WriteTimeout,
                DtrEnable = options.DtrEnable,
                RtsEnable = options.RtsEnable,
            };
        }

        public string PortName { get; }

        public bool IsOpen => _serialPort.IsOpen;

        public event EventHandler<byte[]>? DataReceived;

        public event EventHandler<Exception>? ErrorOccurred;

        public Task OpenAsync(CancellationToken cancellationToken = default)
        {
            if (_serialPort.IsOpen)
                return Task.CompletedTask;

            _serialPort.Open();

            _receiveTask = ReceiveLoopAsync(_receiveCts.Token);

            return Task.CompletedTask;
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            _receiveCts.Cancel();

            if (_receiveTask != null)
            {
                try { await _receiveTask; } catch { }
            }

            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return await _serialPort.BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            _serialPort.Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead > 0)
                    {
                        var data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        DataReceived?.Invoke(this, data);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (TimeoutException)
                {
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, ex);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _receiveCts.Cancel();
            _receiveCts.Dispose();

            if (_serialPort.IsOpen)
                _serialPort.Close();

            _serialPort.Dispose();
        }
    }
}
