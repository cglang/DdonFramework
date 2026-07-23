using Avalonia.Threading;
using Ddon.Desktop.Core.Bridge;
using Ddon.VitrinPLC.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Services;

/// <summary>
/// 监听所有 PLC 会话的点位值变化，仅在值实际变化时通过 IUiBridge 向前端推送事件。
/// 取代原来的定时轮询推送机制。
/// </summary>
public sealed class TagChangeMonitorService : BackgroundService
{
    private readonly IPlcHub _hub;
    private readonly IUiBridge _bridge;
    private readonly ILogger<TagChangeMonitorService> _logger;
    private readonly Dictionary<string, List<IDisposable>> _plcSubscriptions = new(StringComparer.OrdinalIgnoreCase);

    public TagChangeMonitorService(
        IPlcHub hub,
        IUiBridge bridge,
        ILogger<TagChangeMonitorService> logger)
    {
        _hub = hub;
        _bridge = bridge;
        _logger = logger;
    }

    private int _pendingChanges;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 等待系统初始化完成
        await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                RefreshSubscriptions();

                // 有变化待推送，归并后通过 UI 线程推送一次
                if (Interlocked.Exchange(ref _pendingChanges, 0) > 0)
                {
                    await Dispatcher.UIThread.InvokeAsync(
                        () => _bridge.PublishAsync(new TagValuesUpdated()));
                }

                await Task.Delay(500, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "点位变化监控异常");
            }
        }
    }

    private void RefreshSubscriptions()
    {
        var activeNames = new HashSet<string>(_hub.Names, StringComparer.OrdinalIgnoreCase);

        // 清理已移除 PLC 的陈旧订阅
        foreach (var name in _plcSubscriptions.Keys.ToList())
        {
            if (!activeNames.Contains(name))
            {
                if (_plcSubscriptions.Remove(name, out var subs))
                {
                    foreach (var sub in subs) sub.Dispose();
                }
                _logger.LogDebug("已清理 PLC '{Name}' 的变化订阅", name);
            }
        }

        // 为新 PLC 建立点位变化订阅
        foreach (var plcName in activeNames)
        {
            if (_plcSubscriptions.ContainsKey(plcName)) continue;

            try
            {
                var session = _hub.For(plcName);
                var subs = new List<IDisposable>();

                foreach (var tag in session.Tags)
                {
                    try
                    {
                        // Subscribe 只在值实际变化时触发回调（PlcSyncEngine 内部已做新旧值比较）
                        var sub = session.Subscribe<object>(tag.Name, (_, _) =>
                        {
                            Interlocked.Increment(ref _pendingChanges);
                        });
                        subs.Add(sub);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogTrace(ex, "订阅点位 '{Tag}' 变化失败", tag.Name);
                    }
                }

                _plcSubscriptions[plcName] = subs;
                _logger.LogDebug("已订阅 PLC '{Name}' 的 {Count} 个点位变化", plcName, subs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "获取 PLC '{Name}' 会话失败", plcName);
            }
        }
    }
}

/// <summary>点位值已更新事件（用作前端自动刷新信号）</summary>
public sealed class TagValuesUpdated { }
