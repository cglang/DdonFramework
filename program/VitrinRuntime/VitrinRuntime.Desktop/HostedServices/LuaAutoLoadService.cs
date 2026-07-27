using Ddon.LuaEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VitrinRuntime.Desktop.Services;
using VitrinRuntime.Desktop.Stores;

namespace VitrinRuntime.Desktop.HostedServices;

public sealed class LuaAutoLoadService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LuaAutoLoadService> _logger;

    public LuaAutoLoadService(IServiceProvider serviceProvider, ILogger<LuaAutoLoadService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(500, stoppingToken);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var scriptManager = scope.ServiceProvider.GetRequiredService<ILuaScriptManager>();
            var store = scope.ServiceProvider.GetRequiredService<ILuaGroupStore>();
            var eventBridge = scope.ServiceProvider.GetRequiredService<LuaEventBridgeService>();

            var groups = store.GetAll();

            foreach (var config in groups)
            {
                if (stoppingToken.IsCancellationRequested) break;

                if (!Directory.Exists(config.DirectoryPath))
                {
                    _logger.LogWarning("Lua 脚本目录不存在，跳过自动加载: {Path}", config.DirectoryPath);
                    store.Remove(config.GroupName);
                    continue;
                }

                try
                {
                    scriptManager.LoadScriptsFromDirectory(config.DirectoryPath, config.GroupName);
                    eventBridge.SubscribeGroup(config.GroupName);
                    _logger.LogInformation("已自动加载 Lua 脚本组: {Name} ({Path})", config.GroupName, config.DirectoryPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "自动加载 Lua 脚本组失败: {Name}", config.GroupName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lua 自动加载服务初始化失败");
        }
    }
}
