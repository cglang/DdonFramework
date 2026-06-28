using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Configuration;
using Ddon.OpenProtocol.Models;
using Ddon.Socket.Abstractions;
using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.KeepAlive;
using OpenProtocolInterpreter.Tightening;

namespace Ddon.OpenProtocol.Core
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Stopped,
    }

    public class OpenProtocolEndpoint : IOpenProtocolEndpoint, IAsyncDisposable
    {
        private readonly ISocketWorker _worker;
        private readonly OpenProtocolProtocol _protocol;
        private readonly RequestResponseMatcher _matcher;
        private readonly OpenProtocolEventBus _eventBus;
        private readonly OpenProtocolEndpointOptions _options;
        private readonly ILogger<OpenProtocolEndpoint>? _logger;
        private readonly OpenProtocolPipeline? _pipeline;
        private readonly OpenProtocolDispatcher? _dispatcher;

        private volatile ConnectionState _state = ConnectionState.Disconnected;
        private CancellationTokenSource _loopCts = new();

        private Task? _receiveTask;
        private Task? _keepAliveTask;

        private readonly Channel<byte[]> _receiveChannel =
            Channel.CreateUnbounded<byte[]>(
                new UnboundedChannelOptions { SingleReader = true });

        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly SemaphoreSlim _connectLock = new(1, 1);

        private readonly List<Mid> _subscriptions = new List<Mid>();
        private readonly SemaphoreSlim _subscriptionLock = new(1, 1);

        private readonly SemaphoreSlim _reconnectLock = new(1, 1);

        private DateTime _lastSendTime = DateTime.MinValue;
        private bool _disposed;

        public OpenProtocolEndpoint(
            ISocketWorker worker,
            OpenProtocolProtocol protocol,
            RequestResponseMatcher matcher,
            OpenProtocolEventBus eventBus,
            OpenProtocolEndpointOptions options,
            ILogger<OpenProtocolEndpoint>? logger = null,
            OpenProtocolPipeline? pipeline = null,
            OpenProtocolDispatcher? dispatcher = null)
        {
            _worker = worker;
            _protocol = protocol;
            _matcher = matcher;
            _eventBus = eventBus;
            _options = options;
            _logger = logger;
            _pipeline = pipeline;
            _dispatcher = dispatcher;
        }

        public string Name => _options.Name;

        public bool IsConnected => _state == ConnectionState.Connected;

        public ConnectionState State => _state;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OpenProtocolEndpoint));

            await _connectLock.WaitAsync(cancellationToken);
            try
            {
                if (_state is ConnectionState.Connected or ConnectionState.Connecting)
                    return;

                _state = ConnectionState.Connecting;

                _worker.DataReceived += OnDataReceived;
                _worker.Disconnected += OnDisconnected;
                _worker.ErrorOccurred += OnErrorOccurred;

                await _worker.ConnectAsync(cancellationToken);
                _state = ConnectionState.Connected;

                ResetLoopCts();
                var token = _loopCts.Token;

                _receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);
                _keepAliveTask = Task.Run(() => KeepAliveLoopAsync(token), token);

                var mid0001 = new Mid0001();
                await SendAsync<Mid0002>(mid0001, cancellationToken);

                _logger?.LogInformation(
                    "[{Name}] Connected to {Host}:{Port}",
                    _options.Name, _options.Host, _options.Port);

                await ReplaySubscriptionsAsync(cancellationToken);
            }
            finally
            {
                _connectLock.Release();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _connectLock.WaitAsync(cancellationToken);
            try
            {
                if (_state == ConnectionState.Stopped) return;

                _state = ConnectionState.Stopped;

                _logger?.LogInformation("[{Name}] Stopping...", _options.Name);

                _worker.DataReceived -= OnDataReceived;
                _worker.Disconnected -= OnDisconnected;
                _worker.ErrorOccurred -= OnErrorOccurred;

                await StopLoopsAsync();

                await _worker.DisconnectAsync(cancellationToken);
            }
            finally
            {
                _connectLock.Release();
            }
        }

        private async Task StopLoopsAsync()
        {
            _loopCts.Cancel();

            var tasks = new List<Task>(2);
            if (_receiveTask is not null) tasks.Add(_receiveTask);
            if (_keepAliveTask is not null) tasks.Add(_keepAliveTask);

            if (tasks.Count > 0)
            {
                try
                {
                    var allTasks = Task.WhenAll(tasks);
                    var timeout = Task.Delay(TimeSpan.FromSeconds(5));
                    await Task.WhenAny(allTasks, timeout);
                }
                catch { }
            }
        }

        private void ResetLoopCts()
        {
            try { _loopCts.Cancel(); } catch { }
            _loopCts.Dispose();
            _loopCts = new CancellationTokenSource();
        }

        public async Task<TResponse> SendAsync<TResponse>(
            Mid request,
            CancellationToken cancellationToken = default)
            where TResponse : Mid
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException(
                    $"Cannot send: client is {_state}.");

            var (task, _) = _matcher.Enqueue(
                request.Header.Mid, _options.RequestTimeoutMs, cancellationToken);

            await SendInternalAsync(request, cancellationToken);

            try
            {
                Mid response = await task;
                return (TResponse)response;
            }
            catch (TaskCanceledException) when (
                !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"MID{request.Header.Mid:D4} response not received " +
                    $"within {_options.RequestTimeoutMs}ms.");
            }
        }

        public async Task<TResponse> SubscribeAsync<TResponse>(
            Mid request,
            CancellationToken cancellationToken = default)
            where TResponse : Mid
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException(
                    $"Cannot subscribe: client is {_state}.");

            await _subscriptionLock.WaitAsync(cancellationToken);
            try
            {
                _subscriptions.Add(request);
            }
            finally
            {
                _subscriptionLock.Release();
            }

            var (task, _) = _matcher.Enqueue(
                request.Header.Mid, _options.RequestTimeoutMs, cancellationToken);

            await SendInternalAsync(request, cancellationToken);

            try
            {
                Mid response = await task;
                return (TResponse)response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RegisterSubscriptionAsync(
            Mid request,
            CancellationToken cancellationToken = default)
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException(
                    $"Cannot register subscription: client is {_state}.");

            await _subscriptionLock.WaitAsync(cancellationToken);
            try
            {
                _subscriptions.Add(request);
            }
            finally
            {
                _subscriptionLock.Release();
            }

            await SendInternalAsync(request, cancellationToken);
        }

        public IDisposable Subscribe<TMid>(Func<TMid, Task> handler)
            where TMid : Mid
            => _eventBus.Subscribe(handler);

        public IDisposable Subscribe<TMid>(Action<TMid> handler)
            where TMid : Mid
            => _eventBus.Subscribe(handler);

        public IDisposable SubscribeAll(Func<Mid, Task> handler)
            => _eventBus.SubscribeAll(handler);

        private async Task SendInternalAsync(Mid mid, CancellationToken ct)
        {
            byte[] data = _protocol.Serialize(mid);

            await _sendLock.WaitAsync(ct);
            try
            {
                _logger?.LogTrace(
                    "[{Name}] → MID{Mid:D4} ({Len} bytes)",
                    _options.Name, mid.Header.Mid, data.Length);

                await _worker.SendAsync(data, 0, data.Length, ct);
                _lastSendTime = DateTime.UtcNow;
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private void OnDataReceived(object? sender, byte[] data)
        {
            _receiveChannel.Writer.TryWrite(data);
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            var previousState = _state;

            if (previousState == ConnectionState.Stopped)
                return;

            _state = ConnectionState.Disconnected;

            _logger?.LogWarning("[{Name}] Disconnected", _options.Name);

            _matcher.FailAll(new IOException(
                $"[{_options.Name}] Connection lost"));

            if (_options.AutoReconnect && previousState != ConnectionState.Stopped)
            {
                _ = ReconnectLoopAsync(CancellationToken.None);
            }
        }

        private void OnErrorOccurred(object? sender, Exception ex)
        {
            _logger?.LogError(ex, "[{Name}] Socket error", _options.Name);
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            _logger?.LogDebug("[{Name}] ReceiveLoop started", _options.Name);

            var framer = new PacketFramer();

            try
            {
                await foreach (var chunk in _receiveChannel.Reader.ReadAllAsync(ct))
                {
                    framer.Feed(chunk);

                    while (framer.TryReadPacket(out byte[]? packet))
                    {
                        await ProcessFrameAsync(packet!, ct);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[{Name}] ReceiveLoop fatal error", _options.Name);
                _matcher.FailAll(ex);
            }
            finally
            {
                _logger?.LogDebug("[{Name}] ReceiveLoop exited", _options.Name);
            }
        }

        private async Task ProcessFrameAsync(byte[] packet, CancellationToken ct)
        {
            try
            {
                Mid? mid = _protocol.Deserialize(packet);

                if (mid is Mid0061 m61)
                {
                    _logger?.LogDebug(
                        "[{Name}] [MID0061] Torque={T} Angle={A} Status={S}",
                        _options.Name, m61.Torque, m61.Angle, m61.TighteningStatus);
                }

                if (mid is null)
                {
                    _logger?.LogWarning(
                        "[{Name}] Parse returned null, skipped",
                        _options.Name);
                    return;
                }

                _logger?.LogDebug(
                    "[{Name}] ← MID{Mid:D4} ({Len} bytes)",
                    _options.Name, mid.Header.Mid, packet.Length);

                bool matched = _matcher.TryComplete(mid);
                _logger?.LogDebug(
                    "[{Name}] TryComplete(MID{Mid:D4}) = {Matched}",
                    _options.Name, mid.Header.Mid, matched);

                if (!matched)
                {
                    var context = new OpenProtocolContext
                    {
                        ConnectionName = _options.Name,
                        Buffer = packet,
                        Length = packet.Length,
                        ReceiveTime = DateTime.UtcNow,
                        ParsedMessage = mid,
                    };

                    if (_pipeline is not null)
                        await _pipeline.ExecuteAsync(context);

                    _eventBus.Publish(mid);

                    if (_dispatcher is not null)
                        await _dispatcher.DispatchAsync(context, ct);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[{Name}] Error processing frame", _options.Name);
            }
        }

        private async Task KeepAliveLoopAsync(CancellationToken ct)
        {
            _logger?.LogDebug("[{Name}] KeepAliveLoop started", _options.Name);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(1_000, ct);

                    if (_state != ConnectionState.Connected) continue;

                    if ((DateTime.UtcNow - _lastSendTime).TotalMilliseconds
                        >= _options.KeepAliveIntervalMs)
                    {
                        _logger?.LogTrace(
                            "[{Name}] Sending KeepAlive MID9999", _options.Name);

                        try
                        {
                            await SendInternalAsync(new Mid9999(), ct);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(
                                ex, "[{Name}] KeepAlive send failed", _options.Name);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[{Name}] KeepAliveLoop error", _options.Name);
            }
            finally
            {
                _logger?.LogDebug("[{Name}] KeepAliveLoop exited", _options.Name);
            }
        }

        private async Task ReconnectLoopAsync(CancellationToken ct)
        {
            if (!await _reconnectLock.WaitAsync(0))
                return;

            try
            {
                _state = ConnectionState.Reconnecting;

                _logger?.LogWarning("[{Name}] Reconnecting...", _options.Name);

                int delayMs = _options.ReconnectBaseMs;

                while (_state != ConnectionState.Stopped && !ct.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(delayMs, ct);

                        _logger?.LogInformation(
                            "[{Name}] Reconnect attempt in {Delay}ms",
                            _options.Name, delayMs);

                        await StopLoopsAsync();
                        ResetLoopCts();

                        _worker.DataReceived += OnDataReceived;
                        _worker.Disconnected += OnDisconnected;
                        _worker.ErrorOccurred += OnErrorOccurred;

                        await _worker.ConnectAsync(ct);
                        _state = ConnectionState.Connected;

                        var token = _loopCts.Token;
                        _receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);
                        _keepAliveTask = Task.Run(() => KeepAliveLoopAsync(token), token);

                        await SendAsync<Mid0002>(new Mid0001(), ct);

                        _logger?.LogInformation(
                            "[{Name}] Reconnected successfully", _options.Name);

                        await ReplaySubscriptionsAsync(ct);

                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex,
                            "[{Name}] Reconnect failed. Next in {Delay}ms",
                            _options.Name, delayMs);

                        delayMs = Math.Min(delayMs * 2, _options.ReconnectMaxMs);
                    }
                }
            }
            catch { }
            finally
            {
                _reconnectLock.Release();
            }
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
                        await SendInternalAsync(sub, ct);
                        _logger?.LogDebug(
                            "[{Name}] Replayed subscription MID{Mid:D4}",
                            _options.Name, sub.Header.Mid);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex,
                            "[{Name}] Failed to replay subscription MID{Mid:D4}",
                            _options.Name, sub.Header.Mid);
                    }
                }
            }
            finally
            {
                _subscriptionLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            if (_state != ConnectionState.Stopped)
            {
                try { await StopAsync(); } catch { }
            }

            await _eventBus.DisposeAsync();
            _loopCts.Dispose();
            _sendLock.Dispose();
            _connectLock.Dispose();
            _subscriptionLock.Dispose();
            _reconnectLock.Dispose();
            _worker.Dispose();
        }
    }
}
