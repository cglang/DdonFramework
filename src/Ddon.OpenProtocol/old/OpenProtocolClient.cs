using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenProtocol.Events;
using OpenProtocol.Framing;
using OpenProtocol.Pending;
using OpenProtocolInterpreter;                 // NuGet: OpenProtocolInterpreter
using OpenProtocolInterpreter.Communication;   // Mid0001, Mid0002, Mid0003
using OpenProtocolInterpreter.KeepAlive;       // Mid9999
using OpenProtocolInterpreter.Tightening;

namespace Ddon.OpenProtocol;

// ─── Connection State ─────────────────────────────────────────────

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Stopped,
}

// ─── Client Options ───────────────────────────────────────────────

public sealed class OpenProtocolClientOptions
{
    public string Name { get; set; } = "default";
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 4545;
    public int ConnectTimeoutMs { get; set; } = 500_000;
    public int RequestTimeoutMs { get; set; } = 5_000;
    public int KeepAliveIntervalMs { get; set; } = 10_000;
    public int ReconnectBaseMs { get; set; } = 1_000;
    public int ReconnectMaxMs { get; set; } = 30_000;
    public bool AutoReconnect { get; set; } = true;
}

// ─── Client Interface ─────────────────────────────────────────────

public interface IOpenProtocolClient
{
    ConnectionState State { get; }
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// 发送请求并等待对应的响应 MID。
    /// TResponse 必须是 OpenProtocolInterpreter 库中的 Mid 子类。
    /// </summary>
    Task<TResponse> SendAsync<TResponse>(
        Mid request,
        CancellationToken ct = default)
        where TResponse : Mid;

    // ── 订阅 Push 事件（Controller 主动推送的 MID）──────────────
    IDisposable Subscribe<TMid>(Func<TMid, Task> handler) where TMid : Mid;
    IDisposable Subscribe<TMid>(Action<TMid> handler) where TMid : Mid;
    IDisposable SubscribeAll(Func<Mid, Task> handler);

    /// <summary>
    /// 注册订阅请求（自动重连后会重发）
    /// </summary>
    Task RegisterSubscriptionAsync(Mid subscribeRequest, CancellationToken ct = default);
}

// ─── Client Implementation ────────────────────────────────────────

/// <summary>
/// 工业级 Open Protocol TCP Client
///
/// 协议解析完全委托给 NuGet 包 OpenProtocolInterpreter（MidInterpreter）。
/// 本 Client 只负责：TCP 生命周期 / 拆包 / 路由 / 重连 / KeepAlive / 事件分发。
///
/// 后台循环：
///   ReceiveLoop   → PacketFramer → MidInterpreter.Parse → PendingManager | EventBus
///   SendLoop      → Channel{SendItem} → NetworkStream（单线程写）
///   KeepAliveLoop → 无消息时发 Mid9999
///   ReconnectLoop → 断线时指数退避重连
/// </summary>
public sealed class OpenProtocolClient : IOpenProtocolClient, IAsyncDisposable
{
    private readonly OpenProtocolClientOptions _options;
    private readonly MidInterpreter _interpreter; // ← NuGet 库
    private readonly PendingRequestManager _pending;
    private readonly OpenProtocolEventBus _eventBus;
    private readonly ILogger<OpenProtocolClient> _logger;

    // 发送队列：多写单读，SendLoop 是唯一消费者
    private readonly Channel<SendItem> _sendChannel =
        Channel.CreateUnbounded<SendItem>(
            new UnboundedChannelOptions { SingleReader = true });

    // 已注册订阅列表（断线重连后重发）
    private readonly List<Mid> _subscriptions = [];
    private readonly SemaphoreSlim _subscriptionLock = new(1, 1);

    // TCP & Pipeline
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private PipeReader? _pipeReader;

    // 后台 Task
    private Task? _receiveTask;
    private Task? _sendTask;
    private Task? _keepAliveTask;
    private CancellationTokenSource _loopCts = new();

    // 连接状态（volatile，无锁读）
    private volatile ConnectionState _state = ConnectionState.Disconnected;

    // 最后一次发送时间（KeepAlive 判断）
    private DateTime _lastSendTime = DateTime.MinValue;

    // 防止 Connect / Disconnect 并发
    private readonly SemaphoreSlim _connectLock = new(1, 1);

    public ConnectionState State => _state;
    public bool IsConnected => _state == ConnectionState.Connected;

    // ─── Constructor ──────────────────────────────────────────────

    /// <param name="interpreter">
    ///   已配置好 UseAllMessages() / UseIntegratorMessages() 的 MidInterpreter 实例。
    ///   由调用方（DI 或手动构造）创建并注入，保持单例。
    /// </param>
    public OpenProtocolClient(
        OpenProtocolClientOptions options,
        MidInterpreter interpreter,
        OpenProtocolEventBus eventBus,
        ILogger<OpenProtocolClient> logger)
    {
        _options = options;
        _interpreter = interpreter;
        _pending = new PendingRequestManager();
        _eventBus = eventBus;
        _logger = logger;
    }

    // ─── Connect ──────────────────────────────────────────────────

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _connectLock.WaitAsync(ct);
        try
        {
            if (_state is ConnectionState.Connected or ConnectionState.Connecting)
                return;

            _state = ConnectionState.Connecting;
            await ConnectInternalAsync(ct);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task ConnectInternalAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_options.Host) || _options.Port <= 0)
        {
            return;
        }

        _logger.LogInformation(
            "[OpenProtocol] Connecting to {Host}:{Port}...",
            _options.Host, _options.Port);

        _tcp = new TcpClient { NoDelay = true };
        _stream = null;
        _pipeReader = null;

        using var timeoutCts = new CancellationTokenSource(_options.ConnectTimeoutMs);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutCts.Token, ct);

        await _tcp.ConnectAsync(_options.Host, _options.Port, linked.Token);

        _stream = _tcp.GetStream();
        _pipeReader = PipeReader.Create(_stream);

        // 取消旧循环（重连场景）
        _loopCts.Cancel();
        _loopCts.Dispose();
        _loopCts = new CancellationTokenSource();

        var loopToken = _loopCts.Token;

        _receiveTask = Task.Run(() => ReceiveLoop(loopToken), loopToken);
        _sendTask = Task.Run(() => SendLoop(loopToken), loopToken);
        _keepAliveTask = Task.Run(() => KeepAliveLoop(loopToken), loopToken);

        // ── MID0001 握手 ──────────────────────────────────────────
        // 库中 Mid0001 构造后直接使用，属性按需设置
        var mid0001 = new Mid0001();
        // 可选：mid0001.KeepAliveTime = _options.KeepAliveIntervalMs / 1000;

        var (handshakeTask, _) = _pending.Enqueue(Mid0001.MID, _options.RequestTimeoutMs, ct);

        await SendRawAsync(mid0001, ct);

        _ = await handshakeTask; // 等待 Mid0002

        _state = ConnectionState.Connected;

        _logger.LogInformation("[OpenProtocol] Connected successfully.");

        // 重连后重发订阅
        await ReplaySubscriptionsAsync(ct);
    }

    // ─── Disconnect ───────────────────────────────────────────────

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        await _connectLock.WaitAsync(ct);
        try
        {
            if (_state == ConnectionState.Stopped) return;

            _state = ConnectionState.Stopped;

            try { await SendRawAsync(new Mid0003(), ct); }
            catch { /* 尽力发送，失败忽略 */ }

            await StopLoopsAsync();

            _logger.LogInformation("[OpenProtocol] Disconnected.");
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task StopLoopsAsync()
    {
        _loopCts.Cancel();

        var tasks = new[] { _receiveTask, _sendTask, _keepAliveTask }
            .Where(t => t is not null)
            .Cast<Task>();

        try
        {
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch
        {
            /* 忽略取消 / 超时 */
        }

        _pipeReader?.Complete();
        _stream?.Dispose();
        _tcp?.Dispose();
    }

    // ─── SendAsync ────────────────────────────────────────────────

    // TODO: 如果超时 尝试接收 MID0004
    public async Task<TResponse> SendAsync<TResponse>(
        Mid request,
        CancellationToken ct = default)
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
        catch (Exception)
        {
            throw;
        }
    }

    // ─── Subscriptions ────────────────────────────────────────────

    public IDisposable Subscribe<TMid>(Func<TMid, Task> handler) where TMid : Mid
        => _eventBus.Subscribe(handler);

    public IDisposable Subscribe<TMid>(Action<TMid> handler) where TMid : Mid
        => _eventBus.Subscribe(handler);

    public IDisposable SubscribeAll(Func<Mid, Task> handler)
        => _eventBus.SubscribeAll(handler);

    public async Task RegisterSubscriptionAsync(
        Mid subscribeRequest,
        CancellationToken ct = default)
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
                    _logger.LogDebug(
                        "[OpenProtocol] 已重新播放的订阅 MID{Mid:D4}",
                        sub.Header.Mid);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "[OpenProtocol] 无法重放订阅 MID{Mid:D4}",
                        sub.Header.Mid);
                }
            }
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    // ─── Receive Loop ─────────────────────────────────────────────

    private async Task ReceiveLoop(CancellationToken ct)
    {
        _logger.LogDebug("[OpenProtocol] ReceiveLoop started.");
        try
        {
            var framer = new PacketFramer(_pipeReader!);

            await foreach (var packet in framer.ReadPacketsAsync(ct))
            {
                // 每个包单独 try，解析失败只跳过这一帧，不影响后续帧
                try
                {
                    Mid? mid = _interpreter.Parse(packet);

                    if (mid is Mid0061 m61)
                    {
                        await SendRawAsync(new Mid0062(), ct);
                    }
                    //else if (mid is Mid7410 m10)
                    //{
                    //    await SendRawAsync(new Mid7411(), ct);
                    //}
                    else if (mid is Mid0005)
                    {
                        _logger.LogDebug($"[MID0005 RAW] {mid.Header.Mid}");
                    }

                    if (mid is null)
                    {
                        _logger.LogWarning("[OpenProtocol] Parse returned null, skipped. Raw MID={Raw}", Encoding.ASCII.GetString(packet[4..8]));
                        continue;
                    }

                    _logger.LogTrace("[OpenProtocol] [收到数据] MID{Mid:D4} ({Len} bytes)", mid.Header.Mid, packet.Length);

                    // 先尝试匹配等待中的 Request/Response
                    bool matched = _pending.TryComplete(mid.Header.Mid, mid);

                    if (!matched)
                    {
                        // 未匹配 → Push 事件，投入 EventBus（非阻塞）
                        _eventBus.Publish(mid);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OpenProtocol] Error processing packet, skipped.");
                }
            }
        }
        catch (OperationCanceledException) { /* 正常退出 */ }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpenProtocol] ReceiveLoop fatal error.");

            _pending.FailAll(ex);

            if (_options.AutoReconnect && _state != ConnectionState.Stopped)
                _ = Task.Run(() => ReconnectLoop(CancellationToken.None));
        }
        finally
        {
            _logger.LogDebug("[OpenProtocol] ReceiveLoop exited.");
        }
    }

    // ─── SafeParse ────────────────────────────────────────────────

    /// <summary>
    /// 安全解析：
    ///   1. 优先正常解析
    ///   2. 返回 null 或抛异常（库 bug / 不支持的 revision）时，
    ///      自动从高到低降级 revision 重试
    ///   3. 全部失败返回 null，由调用方决定如何处理（跳过该帧）
    /// </summary>
    private Mid? SafeParse(byte[] packet)
    {
        // 正常解析
        try
        {
            var mid = _interpreter.Parse(packet);
            if (mid is not null) return mid;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                "[OpenProtocol] Parse failed (rev={Rev}), will retry with lower revision. {Msg}",
                Encoding.ASCII.GetString(packet[8..11]), ex.Message);
        }

        // revision 不支持或库 bug（如 TighteningErrorStatus2 NullRef）
        // 逐级降级重试：6 → 5 → 4 → 3 → 2 → 1
        byte[] patched = (byte[])packet.Clone();
        foreach (int rev in new[] { 6, 5, 4, 3, 2, 1 })
        {
            try
            {
                string revStr = rev.ToString("D3");
                patched[8] = (byte)revStr[0];
                patched[9] = (byte)revStr[1];
                patched[10] = (byte)revStr[2];

                var mid = _interpreter.Parse(patched);
                if (mid is not null)
                {
                    _logger.LogDebug(
                        "[OpenProtocol] MID{Raw} parsed with downgraded revision {Rev}.",
                        Encoding.ASCII.GetString(packet[4..8]), rev);
                    return mid;
                }
            }
            catch { /* 继续降级 */ }
        }

        return null;
    }

    // ─── Send Loop ────────────────────────────────────────────────

    private async Task SendLoop(CancellationToken ct)
    {
        _logger.LogDebug("[OpenProtocol] SendLoop started.");
        try
        {
            await foreach (var item in _sendChannel.Reader.ReadAllAsync(ct))
            {
                if (item.Tcs.Task.IsCompleted) continue; // 已超时，跳过

                try
                {
                    await _stream!.WriteAsync(item.Data, ct);
                    await _stream.FlushAsync(ct);

                    _lastSendTime = DateTime.UtcNow;

                    _logger.LogTrace(
                        "[OpenProtocol] → {Len} bytes sent.", item.Data.Length);

                    item.Tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    item.Tcs.TrySetException(ex);
                    throw; // 传播到 SendLoop catch，触发重连
                }
            }
        }
        catch (OperationCanceledException) { /* 正常退出 */ }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpenProtocol] SendLoop fatal error.");
        }
        finally
        {
            _logger.LogDebug("[OpenProtocol] SendLoop exited.");
        }
    }

    // ─── KeepAlive Loop ───────────────────────────────────────────

    private async Task KeepAliveLoop(CancellationToken ct)
    {
        _logger.LogDebug("[OpenProtocol] KeepAliveLoop started.");
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(1_000, ct);

                if (_state != ConnectionState.Connected) continue;

                if ((DateTime.UtcNow - _lastSendTime).TotalMilliseconds >= _options.KeepAliveIntervalMs)
                {
                    _logger.LogTrace("[OpenProtocol] Sending KeepAlive MID9999.");
                    await SendRawAsync(new Mid9999(), ct);
                }
            }
        }
        catch (OperationCanceledException) { /* 正常退出 */ }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OpenProtocol] KeepAliveLoop error.");
        }
        finally
        {
            _logger.LogDebug("[OpenProtocol] KeepAliveLoop exited.");
        }
    }

    // ─── Reconnect Loop ───────────────────────────────────────────

    private async Task ReconnectLoop(CancellationToken ct)
    {
        _state = ConnectionState.Reconnecting;

        int delay = _options.ReconnectBaseMs;

        _logger.LogWarning("[OpenProtocol] Connection lost. Reconnecting...");

        while (_state != ConnectionState.Stopped && !ct.IsCancellationRequested)
        {
            try
            {
                await StopLoopsAsync();

                _logger.LogInformation("[OpenProtocol] Reconnect attempt in {Delay}ms...", delay);

                await Task.Delay(delay, ct);

                await ConnectInternalAsync(ct);

                _logger.LogInformation("[OpenProtocol] Reconnected.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[OpenProtocol] Reconnect failed. Next in {Delay}ms.", delay);

                // 指数退避，上限 ReconnectMaxMs
                delay = Math.Min(delay * 2, _options.ReconnectMaxMs);
            }
        }
    }

    // ─── SendRaw ──────────────────────────────────────────────────

    /// <summary>
    /// 将 Mid 序列化并投入 SendChannel。
    /// 使用库的 Pack() 方法序列化为 Open Protocol ASCII 字符串，再转 byte[]。
    /// </summary>
    private async Task SendRawAsync(Mid mid, CancellationToken ct = default)
    {
        // ── 关键：使用库的 Pack() 序列化 ──
        // Pack() 返回完整的 Open Protocol ASCII 字符串（含长度头 + NUL）
        string packed = mid.Pack() + '\0';
        byte[] data = Encoding.ASCII.GetBytes(packed);

        _logger.LogDebug($"[发送数据] [Mid{mid.Header.Mid}] {packed}");

        if (mid.Header.Mid == 224)
        {
            _logger.LogInformation($"[发送数据] [反转信号] [Mid{mid.Header.Mid}] {packed}");
        }
        var item = new SendItem(data);
        await _sendChannel.Writer.WriteAsync(item, ct);

        // 等待 SendLoop 实际写出（或失败/超时）
        await item.Tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(_options.RequestTimeoutMs), ct);
    }

    // ─── Dispose ──────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_state != ConnectionState.Stopped)
        {
            try { await DisconnectAsync(); } catch { /* 忽略 */ }
        }

        await _eventBus.DisposeAsync();

        _loopCts.Dispose();
        _connectLock.Dispose();
        _subscriptionLock.Dispose();
    }

    // ─── Inner Types ──────────────────────────────────────────────

    private sealed class SendItem(byte[] data)
    {
        public byte[] Data { get; } = data;
        public TaskCompletionSource<bool> Tcs { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
