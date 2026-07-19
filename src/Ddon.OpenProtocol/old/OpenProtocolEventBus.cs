using OpenProtocolInterpreter; // NuGet: Mid base class
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace OpenProtocol.Events;

/// <summary>
/// 轻量级进程内事件总线
/// 用于分发 Controller 主动推送的 MID（如 MID0061、MID0072）
///
/// 设计要点：
///   - ReceiveLoop 仅将事件投入 Channel，立即返回（不阻塞接收）
///   - DispatchLoop 在独立后台线程消费并调用订阅者
///   - 订阅者异常不会影响其他订阅者和 ReceiveLoop
/// </summary>
public sealed class OpenProtocolEventBus : IAsyncDisposable
{
    private readonly System.Threading.Channels.Channel<Mid> _channel =
        System.Threading.Channels.Channel.CreateBounded<Mid>(
            new System.Threading.Channels.BoundedChannelOptions(1024)
            {
                FullMode = System.Threading.Channels.BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
            });

    private readonly ConcurrentDictionary<int, List<Func<Mid, Task>>>
        _handlers = new();

    private readonly Task _dispatchTask;
    private readonly CancellationTokenSource _cts = new();

    public OpenProtocolEventBus()
    {
        _dispatchTask = Task.Run(DispatchLoop);
    }

    // ─── Subscribe ────────────────────────────────────────────────

    /// <summary>
    /// 订阅指定 MID 的推送事件
    /// </summary>
    public IDisposable Subscribe<TMid>(Func<TMid, Task> handler)
        where TMid : Mid
    {
        // 通过 new TMid() 获取 MID 号（需无参构造）
        // NuGet 库用静态常量 MID（如 Mid0061.MID）或实例属性 MidNumber
        int midNumber = ((TMid)Activator.CreateInstance(typeof(TMid))!).Header.Mid;

        return SubscribeByMid(midNumber, mid => handler((TMid)mid));
    }

    /// <summary>
    /// 订阅指定 MID 号的推送事件（同步 handler 版本）
    /// </summary>
    public IDisposable Subscribe<TMid>(Action<TMid> handler)
        where TMid : Mid
        => Subscribe<TMid>(mid =>
        {
            handler(mid);
            return Task.CompletedTask;
        });

    /// <summary>
    /// 订阅所有 MID（用于日志、监控等）
    /// </summary>
    public IDisposable SubscribeAll(Func<Mid, Task> handler)
        => SubscribeByMid(-1, handler);

    private IDisposable SubscribeByMid(int midNumber, Func<Mid, Task> handler)
    {
        var list = _handlers.GetOrAdd(midNumber, _ => []);

        lock (list) { list.Add(handler); }

        return new Subscription(() =>
        {
            lock (list) { list.Remove(handler); }
        });
    }

    // ─── Publish ──────────────────────────────────────────────────

    /// <summary>
    /// 投入事件（由 ReceiveLoop 调用，非阻塞）
    /// </summary>
    public void Publish(Mid mid)
    {
        _channel.Writer.TryWrite(mid);
    }

    // ─── Dispatch Loop ────────────────────────────────────────────

    private async Task DispatchLoop()
    {
        await foreach (var mid in _channel.Reader.ReadAllAsync(_cts.Token))
        {
            await InvokeHandlers(mid.Header.Mid, mid);
            await InvokeHandlers(-1, mid); // wildcard subscriptions
        }
    }

    private async Task InvokeHandlers(int midNumber, Mid mid)
    {
        if (!_handlers.TryGetValue(midNumber, out var list))
            return;

        Func<Mid, Task>[] snapshot;

        lock (list) { snapshot = [.. list]; }

        foreach (var handler in snapshot)
        {
            try
            {
                await handler(mid);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"[EventBus] Handler exception for MID{midNumber:D4}: {ex}");
            }
        }
    }

    // ─── Dispose ──────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _channel.Writer.TryComplete();
        await _dispatchTask.ConfigureAwait(false);
        _cts.Dispose();
    }

    // ─── Inner Types ──────────────────────────────────────────────

    private sealed class Subscription(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
