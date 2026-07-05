using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ddon.Hosting;

public abstract class PeriodicBackgroundTask : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    protected PeriodicBackgroundTask(
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 执行间隔
    /// </summary>
    protected abstract TimeSpan Period { get; }

    /// <summary>
    /// 是否在启动后立即执行一次
    /// </summary>
    protected virtual bool RunImmediately => true;

    /// <summary>
    /// 是否允许并发执行
    /// </summary>
    protected virtual bool AllowConcurrentExecution => false;

    private int _running;

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (RunImmediately)
        {
            await ExecuteInternalAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(Period);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteInternalAsync(stoppingToken);
        }
    }

    private async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        if (!AllowConcurrentExecution)
        {
            if (Interlocked.Exchange(ref _running, 1) == 1)
            {
                return;
            }
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();

            await OnExecuteAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Host 正常停止
            _logger.LogInformation("后台任务 {TaskName} 正常停止.", GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "后台任务 {TaskName} 执行失败.", GetType().Name);
        }
        finally
        {
            if (!AllowConcurrentExecution)
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }
    }

    /// <summary>
    /// 子类实现具体业务
    /// </summary>
    protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);
}
