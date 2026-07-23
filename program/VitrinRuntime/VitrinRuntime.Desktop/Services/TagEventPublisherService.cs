using Avalonia.Threading;
using Ddon.Desktop.Core.Bridge;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Services;

/// <summary>后端定时推送标记值更新事件，WebView 模式下前端通过此事件自动刷新点位数据</summary>
public sealed class TagEventPublisherService : BackgroundService
{
    private readonly IUiBridge _bridge;
    private readonly ILogger<TagEventPublisherService> _logger;

    public TagEventPublisherService(IUiBridge bridge, ILogger<TagEventPublisherService> logger)
    {
        _bridge = bridge;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 延迟 1 秒启动，等待 WebView 加载完毕
        await Task.Delay(1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(2000, stoppingToken);
                // 通过 UI 线程推送事件（WebView.InvokeScript 只能在 UI 线程调用）
                await Dispatcher.UIThread.InvokeAsync(() => _bridge.PublishAsync(new TagValuesUpdated()));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "发布 TagValuesUpdated 事件失败");
            }
        }
    }
}

/// <summary>点位值已更新事件（用作前端自动刷新信号）</summary>
public sealed class TagValuesUpdated { }
