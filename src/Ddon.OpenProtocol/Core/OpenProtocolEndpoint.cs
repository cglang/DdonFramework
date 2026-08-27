using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Configuration;
using Ddon.OpenProtocol.Models;
using Ddon.OpenProtocol.Protocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.KeepAlive;

namespace Ddon.OpenProtocol.Core
{
    /// <summary>
    /// Open Protocol 客户端端点。
    /// 采用严格的「发-收-发-收」单线程模型：发送一个 MID，必然收到一个 MID 作为响应。
    /// 不维护请求/响应映射，收到的 MID 即返回给调用方（<see cref="Mid"/> 根类）。
    /// </summary>
    public class OpenProtocolEndpoint : IOpenProtocolEndpoint
    {
        private readonly string _name;
        private readonly OpenProtocolClientOptions _options;
        private readonly MidInterpreter _interpreter = new MidInterpreter().UseAllMessages();
        private readonly OpenProtocolFrameProtocol _frameProtocol = new();
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
        private readonly CancellationTokenSource _cts = new();
        private readonly object _receiveLock = new();
        private readonly object _responseLock = new();
        private readonly object _subscriptionLock = new();
        private readonly object _replayLock = new();
        private readonly List<byte> _receiveBuffer = new();
        private readonly Dictionary<int, List<Func<Mid, Task>>> _subscriptions = new();
        private readonly List<Mid> _replaySubscriptions = new();
        private readonly ILogger _logger;

        private OpenProtocolConnection? _connection;
        private CancellationTokenSource? _keepAliveCts;
        private Task? _keepAliveTask;
        private TaskCompletionSource<Mid>? _responseTcs;

        private volatile ConnectionState _state = ConnectionState.Disconnected;
        private DateTime _lastSendUtc = DateTime.UtcNow;
        private int _disposed;

        public OpenProtocolEndpoint(string name, OpenProtocolClientOptions options, ILogger? logger = null)
        {
            _name = name;
            _options = options;
            _logger = logger ?? NullLogger<OpenProtocolEndpoint>.Instance;
        }

        public string Name => _name;

        public ConnectionState State => _state;

        public bool IsConnected => _state == ConnectionState.Connected;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _lifecycleLock.WaitAsync(cancellationToken);
            try
            {
                if (_state is ConnectionState.Connected or ConnectionState.Connecting)
                    return;

                ThrowIfDisposed();

                _state = ConnectionState.Connecting;
                try
                {
                    await ConnectCoreAsync(cancellationToken);
                    _state = ConnectionState.Connected;
                    _logger.LogInformation("[OpenProtocol:{Name}] 已连接 {Host}:{Port}.", _name, _options.Host, _options.Port);
                }
                catch
                {
                    _state = ConnectionState.Disconnected;
                    throw;
                }
            }
            finally
            {
                _lifecycleLock.Release();
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await _lifecycleLock.WaitAsync(cancellationToken);
            try
            {
                if (_state == ConnectionState.Stopped)
                    return;

                _state = ConnectionState.Stopped;

                // 发送通信停止 MID0003（发-收，服务器会回 MID0005，此处不等待结果）
                try
                {
                    await _sendLock.WaitAsync(CancellationToken.None);
                    try
                    {
                        await SendCoreAsync(new Mid0003(), CancellationToken.None);
                    }
                    finally
                    {
                        _sendLock.Release();
                    }
                }
                finally
                {
                    await TeardownAsync();
                    FailPending(new InvalidOperationException("连接已停止。"));
                    _logger.LogInformation("[OpenProtocol:{Name}] 已断开连接.", _name);
                }
            }
            finally
            {
                _lifecycleLock.Release();
            }
        }

        /// <summary>
        /// 发送一个请求并等待接收一个响应，返回收到的 <see cref="Mid"/> 根类。
        /// 协议要求发-收交替，上一次请求未响应前再次调用会抛出异常。
        /// </summary>
        public async Task<Mid> SendAsync(Mid request, CancellationToken cancellationToken = default)
        {
            EnsureConnected();

            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                return await SendAndWaitCoreAsync(request, cancellationToken);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// 注册订阅：收到指定类型 MID 时执行 handler，订阅的 MID 不作为普通响应。
        /// </summary>
        public IDisposable Subscribe<TMid>(Func<TMid, Task> handler) where TMid : Mid
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            int midNumber = GetMidNumber<TMid>();
            Func<Mid, Task> wrapper = mid => handler((TMid)mid);

            List<Func<Mid, Task>> list;
            lock (_subscriptionLock)
            {
                if (!_subscriptions.TryGetValue(midNumber, out var existing) || existing == null)
                {
                    existing = new List<Func<Mid, Task>>();
                    _subscriptions[midNumber] = existing;
                }
                list = existing;
                list.Add(wrapper);
            }

            return new Subscription(() =>
            {
                lock (_subscriptionLock)
                {
                    list.Remove(wrapper);
                    if (list.Count == 0)
                        _subscriptions.Remove(midNumber);
                }
            });
        }

        /// <summary>
        /// 发送订阅请求（如 MID0060）并等待其确认响应（请求-响应，如 MID0005）。
        /// 之后服务端推送的 <typeparamref name="TMid"/>（如 MID0061）会执行 <paramref name="handler"/>，
        /// 同时调用 <paramref name="ackHandler"/> 生成确认回复（<typeparamref name="TAckMid"/>，如 MID0062）发送给服务端。
        /// 返回订阅请求的确认响应；若订阅请求发送失败，已注册的订阅会被取消。
        /// </summary>
        public async Task<Mid> SubscribeAsync<TMid, TAckMid>(
            Mid subscribeRequest,
            Func<TMid, Task> handler,
            Func<TAckMid> ackHandler,
            CancellationToken cancellationToken = default)
            where TMid : Mid
            where TAckMid : Mid
        {
            if (subscribeRequest == null) throw new ArgumentNullException(nameof(subscribeRequest));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (ackHandler == null) throw new ArgumentNullException(nameof(ackHandler));

            EnsureConnected();

            // 注册订阅：收到 TMid 时执行 handler，并用 ackHandler 生成确认回复发给服务端
            var subscription = Subscribe<TMid>(mid => HandleSubscribedAsync(mid, handler, ackHandler));
            try
            {
                // 发送订阅请求并等待其确认响应（请求-响应，如 MID0005）
                var response = await SendAsync(subscribeRequest, cancellationToken);

                // 订阅成功：登记为可重放的订阅，断线重连后自动重新发送
                lock (_replayLock)
                {
                    _replaySubscriptions.Add(subscribeRequest);
                }

                return response;
            }
            catch
            {
                subscription.Dispose();
                throw;
            }
        }

        private async Task HandleSubscribedAsync<TMid, TAckMid>(
            TMid mid,
            Func<TMid, Task> handler,
            Func<TAckMid> ackHandler)
            where TMid : Mid
            where TAckMid : Mid
        {
            // 先发送自定义确认回复（如 MID0062）给服务端
            try
            {
                TAckMid ack = ackHandler();
                if (ack != null)
                {
                    await _sendLock.WaitAsync();
                    try
                    {
                        await SendCoreAsync(ack, CancellationToken.None);
                    }
                    finally
                    {
                        _sendLock.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OpenProtocol:{Name}] 发送订阅确认回复失败 MID{Mid:D4}.", _name, mid.Header.Mid);
            }

            // 再执行用户的数据处理
            try
            {
                await handler(mid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OpenProtocol:{Name}] 订阅处理异常 MID{Mid:D4}.", _name, mid.Header.Mid);
            }
        }

        private async Task<Mid> SendAndWaitCoreAsync(Mid request, CancellationToken cancellationToken)
        {
            TaskCompletionSource<Mid>? current;
            lock (_responseLock)
            {
                current = _responseTcs;
                if (current != null)
                    throw new InvalidOperationException("协议要求发-收交替：上一次请求尚未收到响应。");
            }

            var tcs = new TaskCompletionSource<Mid>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_responseLock)
            {
                _responseTcs = tcs;
            }

            using var timeoutCts = new CancellationTokenSource(_options.RequestTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
            using var registration = linkedCts.Token.Register(() =>
            {
                bool isMine;
                lock (_responseLock)
                {
                    isMine = ReferenceEquals(_responseTcs, tcs);
                    if (isMine)
                        _responseTcs = null;
                }

                if (isMine)
                {
                    if (timeoutCts.IsCancellationRequested)
                        tcs.TrySetException(new TimeoutException($"请求 MID {request.Header.Mid:D4} 在 {_options.RequestTimeoutMs}ms 内未收到响应。"));
                    else
                        tcs.TrySetCanceled(cancellationToken);
                }
            });

            try
            {
                await SendCoreAsync(request, cancellationToken);
                return await tcs.Task;
            }
            catch
            {
                lock (_responseLock)
                {
                    if (ReferenceEquals(_responseTcs, tcs))
                        _responseTcs = null;
                }
                tcs.TrySetCanceled();
                throw;
            }
        }

        private async Task ConnectCoreAsync(CancellationToken cancellationToken)
        {
            var connection = new OpenProtocolConnection();
            connection.DataReceived += OnDataReceived;
            connection.Disconnected += OnDisconnected;

            lock (_receiveLock)
            {
                _receiveBuffer.Clear();
            }

            await connection.ConnectAsync(_options.Host, _options.Port, _options.ConnectTimeoutMs, cancellationToken);
            _connection = connection;

            _keepAliveCts?.Cancel();
            _keepAliveCts?.Dispose();
            _keepAliveCts = new CancellationTokenSource();
            _keepAliveTask = KeepAliveLoopAsync(_keepAliveCts.Token);

            try
            {
                // 握手：发送 MID0001，等待响应（MID0002）。发-收模式。
                await _sendLock.WaitAsync(cancellationToken);
                try
                {
                    await SendAndWaitCoreAsync(new Mid0001(), cancellationToken);
                }
                finally
                {
                    _sendLock.Release();
                }

                // 重连后重新发送之前成功的订阅请求（服务端会话已重置）
                await ReplaySubscriptionsAsync(cancellationToken);
            }
            catch
            {
                await TeardownAsync();
                throw;
            }

            _lastSendUtc = DateTime.UtcNow;
        }

        private async Task ReplaySubscriptionsAsync(CancellationToken cancellationToken)
        {
            List<Mid> requests;
            lock (_replayLock)
            {
                requests = new List<Mid>(_replaySubscriptions);
            }

            foreach (var request in requests)
            {
                try
                {
                    await _sendLock.WaitAsync(cancellationToken);
                    try
                    {
                        await SendAndWaitCoreAsync(request, cancellationToken);
                    }
                    finally
                    {
                        _sendLock.Release();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[OpenProtocol:{Name}] 重放订阅失败 MID{Mid:D4}.", _name, request.Header.Mid);
                }
            }
        }

        private async Task TeardownAsync()
        {
            var connection = _connection;
            _connection = null;

            if (connection != null)
            {
                connection.DataReceived -= OnDataReceived;
                connection.Disconnected -= OnDisconnected;
                try
                {
                    await connection.DisconnectAsync();
                }
                catch
                {
                }
                connection.Dispose();
            }

            if (_keepAliveCts != null)
            {
                _keepAliveCts.Cancel();
                if (_keepAliveTask != null)
                {
                    try
                    {
                        await _keepAliveTask;
                    }
                    catch
                    {
                    }
                    _keepAliveTask = null;
                }
                _keepAliveCts.Dispose();
                _keepAliveCts = null;
            }
        }

        private async Task SendCoreAsync(Mid mid, CancellationToken cancellationToken)
        {
            var connection = _connection
                ?? throw new InvalidOperationException("连接未建立。");

            byte[] data = Pack(mid);
            await connection.SendAsync(data, cancellationToken);
            _lastSendUtc = DateTime.UtcNow;
        }

        private byte[] Pack(Mid mid)
        {
            string terminator = _options.Terminator switch
            {
                MessageTerminator.Nul => "\0",
                MessageTerminator.CrLf => "\r\n",
                MessageTerminator.Custom => _options.CustomTerminator,
                _ => string.Empty,
            };

            string packed = FixLengthField(mid.Pack());

            return Encoding.ASCII.GetBytes(packed + terminator);
        }

        private static string FixLengthField(string packed)
        {
            if (packed.Length < 4)
                return packed;

            for (int i = 0; i < 4; i++)
            {
                char c = packed[i];
                if (c < '0' || c > '9')
                    return packed;
            }

            int claimed = 0;
            for (int i = 0; i < 4; i++)
                claimed = claimed * 10 + (packed[i] - '0');

            if (claimed == packed.Length)
                return packed;

            return packed.Length.ToString("D4") + packed.Substring(4);
        }

        private void OnDataReceived(object? sender, byte[] data)
        {
            lock (_receiveLock)
            {
                _receiveBuffer.AddRange(data);
                ProcessAvailableFrames();
            }
        }

        private void ProcessAvailableFrames()
        {
            byte[] buffer = _receiveBuffer.ToArray();
            int offset = 0;
            while (offset < buffer.Length)
            {
                var (frame, consumed) = _frameProtocol.Decode(buffer, offset, buffer.Length - offset);

                if (frame == null)
                {
                    offset += consumed;
                    break;
                }

                offset += consumed;
                ProcessFrame(frame);
            }

            _receiveBuffer.Clear();
            int remaining = buffer.Length - offset;
            if (remaining > 0)
            {
                var leftover = new byte[remaining];
                Array.Copy(buffer, offset, leftover, 0, remaining);
                _receiveBuffer.AddRange(leftover);
            }
        }

        private void ProcessFrame(byte[] frame)
        {
            Mid? message;
            try
            {
                message = _interpreter.Parse(frame);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OpenProtocol:{Name}] 解析 MID 失败: {Raw}.",
                    _name, Encoding.ASCII.GetString(frame));
                return;
            }

            if (message == null)
                return;

            // 订阅优先：已注册订阅的 MID 只投递给 handler，不作为普通响应
            if (TryInvokeSubscriptions(message))
                return;

            // 单线程发-收模型：收到的 MID 就是当前等待请求的响应
            TaskCompletionSource<Mid>? tcs;
            lock (_responseLock)
            {
                tcs = _responseTcs;
                if (tcs != null)
                    _responseTcs = null;
            }

            tcs?.TrySetResult(message);
        }

        private bool TryInvokeSubscriptions(Mid message)
        {
            Func<Mid, Task>[] handlers;
            lock (_subscriptionLock)
            {
                if (!_subscriptions.TryGetValue(message.Header.Mid, out var list) || list.Count == 0)
                    return false;

                handlers = list.ToArray();
            }

            foreach (var handler in handlers)
            {
                try
                {
                    _ = handler(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OpenProtocol:{Name}] 订阅处理异常 MID{Mid:D4}.", _name, message.Header.Mid);
                }
            }

            return true;
        }

        private async Task KeepAliveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1_000, cancellationToken);

                    if (_state != ConnectionState.Connected)
                        continue;

                    if ((DateTime.UtcNow - _lastSendUtc).TotalMilliseconds >= _options.KeepAliveIntervalMs)
                    {
                        await _sendLock.WaitAsync(cancellationToken);
                        try
                        {
                            await SendCoreAsync(new Mid9999(), cancellationToken);
                        }
                        finally
                        {
                            _sendLock.Release();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OpenProtocol:{Name}] 心跳循环异常.", _name);
            }
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            if (_disposed != 0 || _state == ConnectionState.Stopped)
                return;

            _state = ConnectionState.Disconnected;
            FailPending(new InvalidOperationException("连接已断开。"));
            _logger.LogWarning("[OpenProtocol:{Name}] 连接已断开.", _name);

            if (_options.AutoReconnect)
                _ = ReconnectLoopAsync();
        }

        private async Task ReconnectLoopAsync()
        {
            await _lifecycleLock.WaitAsync();
            try
            {
                if (_state is ConnectionState.Connected or ConnectionState.Connecting or ConnectionState.Stopped || _disposed != 0)
                    return;

                _state = ConnectionState.Reconnecting;

                int attempt = 0;
                while (_state == ConnectionState.Reconnecting && _disposed == 0)
                {
                    var delayMs = Math.Min(_options.ReconnectBaseMs * (1 << attempt), _options.ReconnectMaxMs);
                    attempt++;

                    _logger.LogInformation("[OpenProtocol:{Name}] {DelayMs}ms 后重连...", _name, delayMs);

                    try
                    {
                        await Task.Delay(delayMs, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    try
                    {
                        await ConnectCoreAsync(_cts.Token);
                        _state = ConnectionState.Connected;
                        _logger.LogInformation("[OpenProtocol:{Name}] 重连成功.", _name);
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[OpenProtocol:{Name}] 第 {Attempt} 次重连失败.", _name, attempt);
                    }
                }
            }
            finally
            {
                _lifecycleLock.Release();
            }
        }

        private void FailPending(Exception reason)
        {
            TaskCompletionSource<Mid>? tcs;
            lock (_responseLock)
            {
                tcs = _responseTcs;
                _responseTcs = null;
            }
            tcs?.TrySetException(reason);
        }

        private void EnsureConnected()
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException($"无法发送：客户端当前状态为 {_state}。");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed != 0)
                throw new ObjectDisposedException(nameof(OpenProtocolEndpoint));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            try
            {
                _cts.Cancel();
            }
            catch
            {
            }

            FailPending(new ObjectDisposedException(nameof(OpenProtocolEndpoint)));

            lock (_subscriptionLock)
            {
                _subscriptions.Clear();
            }

            lock (_replayLock)
            {
                _replaySubscriptions.Clear();
            }

            var connection = _connection;
            _connection = null;
            try
            {
                connection?.Dispose();
            }
            catch
            {
            }

            try
            {
                _cts.Dispose();
            }
            catch
            {
            }
            _lifecycleLock.Dispose();
            _sendLock.Dispose();
        }

        private static int GetMidNumber<TMid>() where TMid : Mid
        {
            var field = typeof(TMid).GetField("MID", BindingFlags.Public | BindingFlags.Static);
            if (field != null && field.GetValue(null) is int mid)
                return mid;

            return ((TMid)Activator.CreateInstance(typeof(TMid))!).Header.Mid;
        }

        private sealed class Subscription : IDisposable
        {
            private Action? _dispose;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref _dispose, null)?.Invoke();
            }
        }
    }
}
