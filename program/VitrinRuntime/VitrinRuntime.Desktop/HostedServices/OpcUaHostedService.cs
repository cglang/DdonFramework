using Ddon.OpcUaServer.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Desktop.HostedServices;

/// <summary>
/// OPC UA Server 托管服务，应用启动时自动启动 OPC UA Server。
/// </summary>
public sealed class OpcUaHostedService : BackgroundService
{
    private readonly IVitrinUaServer _server;
    private readonly ILogger<OpcUaHostedService> _logger;

    public OpcUaHostedService(IVitrinUaServer server, ILogger<OpcUaHostedService> logger)
    {
        _server = server;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OPC UA HostedService 正在启动...");

        try
        {
            await _server.StartAsync(stoppingToken);
            _logger.LogInformation("OPC UA Server 已通过 HostedService 自动启动。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPC UA Server 通过 HostedService 启动失败，用户可稍后手动启动。");
        }

        // 保持运行直到应用退出
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OPC UA HostedService 正在停止...");

        try
        {
            await _server.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPC UA Server 停止时发生异常。");
        }

        await base.StopAsync(cancellationToken);
    }
}
