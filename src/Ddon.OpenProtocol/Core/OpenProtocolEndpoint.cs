using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Configuration;
using Ddon.OpenProtocol.Models;
using Ddon.OpenProtocol.Protocols;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Core;
using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.KeepAlive;
using OpenProtocolInterpreter.Tightening;


namespace Ddon.OpenProtocol.Core
{
    public class OpenProtocolEndpoint : IOpenProtocolEndpoint, IAsyncDisposable
    {
        private readonly string _name;
        private readonly OpenProtocolClientOptions _options;
        private readonly MidInterpreter _interpreter;
        private readonly PendingRequestManager _pending;
        private readonly OpenProtocolEventBus _eventBus;
        private readonly ILogger<OpenProtocolEndpoint> _logger;
        private readonly ISocketWorker _worker;
        private readonly Ddon.Socket.Abstractions.IReconnectStrategy? _reconnectStrategy;
        private readonly OpenProtocolFrameProtocol _protocol = new();
        private readonly HashSet<int> _knownMids;

        private readonly Channel<MidSendContext> _sendChannel =
            Channel.CreateUnbounded<MidSendContext>(
                new UnboundedChannelOptions { SingleReader = true });

        private readonly List<Mid> _subscriptions = new();
        private readonly SemaphoreSlim _subscriptionLock = new(1, 1);
        private readonly SemaphoreSlim _startLock = new(1, 1);
        private readonly List<byte> _receiveBuffer = new();

        private Task? _sendTask;
        private Task? _keepAliveTask;
        private Task? _reconnectTask;
        private CancellationTokenSource _loopCts = new();

        private volatile ConnectionState _state = ConnectionState.Disconnected;
        private DateTime _lastSendTime = DateTime.MinValue;
        private bool _disposed;

        public OpenProtocolEndpoint(
            string name,
            OpenProtocolClientOptions options,
            MidInterpreter interpreter,
            OpenProtocolEventBus eventBus,
            ISocketWorker worker,
            ILogger<OpenProtocolEndpoint> logger,
            Ddon.Socket.Abstractions.IReconnectStrategy? reconnectStrategy = null)
        {
            _name = name;
            _options = options;
            _interpreter = interpreter;
            _pending = new PendingRequestManager();
            _eventBus = eventBus;
            _worker = worker;
            _logger = logger;
            _reconnectStrategy = reconnectStrategy;
            _knownMids = _pending.GetAllKnownMids();
        }

        public ConnectionState State => _state;

        public string Name => _name;

        public void MapResponse(int requestMid, int responseMid)
        {
            _pending.AddMapping(requestMid, responseMid);
            _knownMids.Add(requestMid);
            _knownMids.Add(responseMid);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _startLock.WaitAsync(cancellationToken);
            try
            {
                if (_state is ConnectionState.Connected or ConnectionState.Connecting)
                    return;

                _state = ConnectionState.Connecting;
                await ConnectInternalAsync(cancellationToken);
            }
            finally
            {
                _startLock.Release();
            }
        }

        private async Task ConnectInternalAsync(CancellationToken ct)
        {
            _logger.LogInformation(
                "[OpenProtocol:{Name}] 正在连接 {Host}:{Port}...",
                _name, _options.Host, _options.Port);

            _loopCts.Cancel();
            _loopCts.Dispose();
            _loopCts = new CancellationTokenSource();

            var loopToken = _loopCts.Token;

            using var timeoutCts = new CancellationTokenSource(_options.ConnectTimeoutMs);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, ct);

            await _worker.ConnectAsync(linked.Token);

            _worker.DataReceived += OnDataReceived;
            _worker.Disconnected += OnDisconnected;

            _sendTask = Task.Run(() => SendLoop(loopToken), loopToken);
            _keepAliveTask = Task.Run(() => KeepAliveLoop(loopToken), loopToken);

            var mid0001 = new Mid0001();
            var (handshakeTask, _) = _pending.Enqueue(Mid0001.MID, _options.RequestTimeoutMs, ct);

            await SendRawAsync(mid0001, ct);
            await handshakeTask;

            _state = ConnectionState.Connected;
            _logger.LogInformation("[OpenProtocol:{Name}] 连接成功.", _name);

            await ReplaySubscriptionsAsync(ct);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _startLock.WaitAsync(cancellationToken);
            try
            {
                if (_state == ConnectionState.Stopped) return;

                _state = ConnectionState.Stopped;

                try { await SendRawAsync(new Mid0003(), cancellationToken); }
                catch { }

                await StopLoopsAsync();
                _logger.LogInformation("[OpenProtocol:{Name}] 已断开.", _name);
            }
            finally
            {
                _startLock.Release();
            }
        }

        private async Task StopLoopsAsync()
        {
            _loopCts.Cancel();

            var tasks = new[] { _sendTask, _keepAliveTask }
                .Where(t => t is not null)
                .Cast<Task>();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (TimeoutException) { }
            catch (OperationCanceledException) { }

            _worker.DataReceived -= OnDataReceived;
            _worker.Disconnected -= OnDisconnected;

            try { await _worker.DisconnectAsync(); }
            catch { }
        }

        public async Task<TResponse> SendAsync<TResponse>(Mid request, CancellationToken ct = default)
            where TResponse : Mid
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException($"Cannot send: client is {_state}.");

            var (task, _) = _pending.Enqueue(request.Header.Mid, _options.RequestTimeoutMs, ct);

            await SendRawAsync(request, ct);

            try
            {
                Mid response = await task;
                return (TResponse)response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<TResponse> SubscribeAsync<TResponse>(Mid subscribeRequest, CancellationToken ct = default)
            where TResponse : Mid
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException($"Cannot subscribe: client is {_state}.");

            var (task, _) = _pending.Enqueue(subscribeRequest.Header.Mid, _options.RequestTimeoutMs, ct);

            await SendRawAsync(subscribeRequest, ct);

            try
            {
                Mid response = await task;
                return (TResponse)response;
            }
            catch
            {
                throw;
            }
        }

        public IDisposable Subscribe<TMid>(Func<TMid, Task> handler) where TMid : Mid
            => _eventBus.Subscribe(handler);

        public IDisposable Subscribe<TMid>(Action<TMid> handler) where TMid : Mid
            => _eventBus.Subscribe(handler);

        public IDisposable SubscribeAll(Func<Mid, Task> handler)
            => _eventBus.SubscribeAll(handler);

        public async Task RegisterSubscriptionAsync(Mid subscribeRequest, CancellationToken ct = default)
        {
            await _subscriptionLock.WaitAsync(ct);
            try { _subscriptions.Add(subscribeRequest); }
            finally { _subscriptionLock.Release(); }

            await SendRawAsync(subscribeRequest, ct);
        }

        private async Task ReplaySubscriptionsAsync(CancellationToken ct)
        {
            await _subscriptionLock.WaitAsync(ct);
            try
            {
                foreach (var sub in _subscriptions)
                {
                    try
                    {
                        await SendRawAsync(sub, ct);
                        _logger.LogDebug("[OpenProtocol:{Name}] 已重放订阅 MID{Mid:D4}",
                            _name, sub.Header.Mid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[OpenProtocol:{Name}] 重放订阅失败 MID{Mid:D4}",
                            _name, sub.Header.Mid);
                    }
                }
            }
            finally
            {
                _subscriptionLock.Release();
            }
        }

        private void OnDataReceived(object? sender, byte[] data)
        {
            lock (_receiveBuffer)
            {
                _receiveBuffer.AddRange(data);
            }
            ProcessAvailableFrames();
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            _state = ConnectionState.Disconnected;
            _logger.LogWarning("[OpenProtocol:{Name}] 连接已断开.", _name);

            _pending.FailAll(new Exception("连接已断开"));

            if (_reconnectStrategy != null && _state != ConnectionState.Stopped && !_disposed)
            {
                _reconnectTask = ReconnectLoopAsync(_loopCts.Token);
            }
        }

        private void ProcessAvailableFrames()
        {
            byte[] buffer;
            lock (_receiveBuffer)
            {
                buffer = _receiveBuffer.ToArray();
            }

            int offset = 0;

            while (offset < buffer.Length)
            {
                var (frame, consumed) = _protocol.Decode(buffer, offset, buffer.Length - offset);

                if (frame == null)
                {
                    lock (_receiveBuffer)
                    {
                        _receiveBuffer.Clear();
                        int remaining = buffer.Length - offset;
                        if (remaining > 0)
                        {
                            var remainBytes = new byte[remaining];
                            Array.Copy(buffer, offset, remainBytes, 0, remaining);
                            _receiveBuffer.AddRange(remainBytes);
                        }
                    }
                    break;
                }

                ProcessFrame(frame);
                offset += consumed;
            }

            if (offset >= buffer.Length)
            {
                lock (_receiveBuffer)
                {
                    _receiveBuffer.Clear();
                }
            }
        }

        private void ProcessFrame(byte[] frame)
        {
            int midNumber;
            try
            {
                midNumber = int.Parse(Encoding.ASCII.GetString(frame, 4, 4));
            }
            catch
            {
                return;
            }

            if (!_knownMids.Contains(midNumber))
                return;

            Mid? mid;
            try
            {
                mid = _interpreter.Parse(frame);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OpenProtocol:{Name}] 解析异常 MID={Raw}",
                    _name, Encoding.ASCII.GetString(frame, 4, 8));
                return;
            }

            if (mid is null)
            {
                _logger.LogWarning("[OpenProtocol:{Name}] 解析返回空 MID={Raw}",
                    _name, Encoding.ASCII.GetString(frame, 4, 8));
                return;
            }

            if (mid is Mid0061)
            {
                _ = SendRawAsync(new Mid0062(), CancellationToken.None);
            }

            _logger.LogTrace("[OpenProtocol:{Name}] 收到 MID{Mid:D4} ({Len} 字节).",
                _name, mid.Header.Mid, frame.Length);

            bool matched = _pending.TryComplete(mid.Header.Mid, mid);

            if (!matched)
            {
                _eventBus.Publish(mid);
            }
        }
        private async Task SendRawAsync(Mid mid, CancellationToken ct)
        {
            var context = new MidSendContext(mid);
            await _sendChannel.Writer.WriteAsync(context, ct);
        }

        private async Task SendLoop(CancellationToken ct)
        {
            _logger.LogDebug("[OpenProtocol:{Name}] 发送循环已启动.", _name);
            try
            {
                await foreach (var item in _sendChannel.Reader.ReadAllAsync(ct))
                {
                    if (item.Tcs.Task.IsCompleted) continue;

                    try
                    {
                        string packed = item.Mid.Pack() + '\0';
                        byte[] data = Encoding.ASCII.GetBytes(packed);

                        await _worker.SendAsync(data, 0, data.Length, ct);
                        _lastSendTime = DateTime.UtcNow;

                        _logger.LogTrace("[OpenProtocol:{Name}] 已发送 MID{Mid:D4} ({Len} 字节).",
                            _name, item.Mid.Header.Mid, data.Length);

                        item.Tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        item.Tcs.TrySetException(ex);
                        throw;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OpenProtocol:{Name}] 发送循环异常.", _name);
            }
            finally
            {
                _logger.LogDebug("[OpenProtocol:{Name}] 发送循环已退出.", _name);
            }
        }

        private async Task KeepAliveLoop(CancellationToken ct)
        {
            _logger.LogDebug("[OpenProtocol:{Name}] 心跳循环已启动.", _name);
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(1_000, ct);

                    if (_state != ConnectionState.Connected) continue;

                    if ((DateTime.UtcNow - _lastSendTime).TotalMilliseconds >= _options.KeepAliveIntervalMs)
                    {
                        _logger.LogTrace("[OpenProtocol:{Name}] 发送心跳 MID9999.", _name);
                        await SendRawAsync(new Mid9999(), ct);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OpenProtocol:{Name}] 心跳循环异常.", _name);
            }
            finally
            {
                _logger.LogDebug("[OpenProtocol:{Name}] 心跳循环已退出.", _name);
            }
        }

        private async Task ReconnectLoopAsync(CancellationToken ct)
        {
            _state = ConnectionState.Reconnecting;
            _logger.LogWarning("[OpenProtocol:{Name}] 正在重连...", _name);

            int retryCount = 0;

            while (_state != ConnectionState.Stopped && !ct.IsCancellationRequested)
            {
                try
                {
                    await StopLoopsAsync();

                    var delay = _reconnectStrategy?.GetNextDelay(retryCount + 1)
                        ?? TimeSpan.FromMilliseconds(
                            Math.Min(
                                _options.ReconnectBaseMs * (1 << retryCount),
                                _options.ReconnectMaxMs));

                    retryCount++;

                    _logger.LogInformation("[OpenProtocol:{Name}] {DelayMs}ms 后重试...",
                        _name, delay.TotalMilliseconds);

                    await Task.Delay(delay, ct);

                    await ConnectInternalAsync(ct);

                    _logger.LogInformation("[OpenProtocol:{Name}] 重连成功.", _name);
                    return;
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[OpenProtocol:{Name}] 第 {Retry} 次重连失败.",
                        _name, retryCount);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            if (_state != ConnectionState.Stopped)
            {
                try { await StopAsync(); }
                catch { }
            }

            await _eventBus.DisposeAsync();

            _loopCts.Dispose();
            _startLock.Dispose();
            _subscriptionLock.Dispose();
            _worker.Dispose();
        }
    }
}
