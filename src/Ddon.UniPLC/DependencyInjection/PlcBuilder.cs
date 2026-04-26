using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Clients;
using Ddon.UniPLC.Clients.Siemens;
using Ddon.UniPLC.Core;
using Ddon.UniPLC.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.UniPLC.DependencyInjection;

/// <summary>
/// PLC 框架配置构建器
/// </summary>
public class PlcBuilder
{
    private readonly IServiceCollection _services;
    private readonly PlcClientFactoryRegistry _factoryRegistry;
    private readonly PlcProvider _provider;
    private readonly List<PlcOptions> _clientConfigs;

    public PlcBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _factoryRegistry = new PlcClientFactoryRegistry();
        _provider = new PlcProvider();
        _clientConfigs = new List<PlcOptions>();

        // 注册默认工厂
        _factoryRegistry.Register("Memory", new MemoryPlcClientFactory());
        _factoryRegistry.Register("Siemens", new SiemensPlcClientFactory());
    }

    /// <summary>
    /// 配置 Siemens PLC
    /// </summary>
    public PlcBuilder UseSiemens(string name, Action<SiemensPlcOptions> configure)
    {
        var options = new SiemensPlcOptions { Name = name };
        configure(options);
        _clientConfigs.Add(options);
        return this;
    }

    /// <summary>
    /// 配置 Siemens PLC（默认名称）
    /// </summary>
    public PlcBuilder UseSiemens(Action<SiemensPlcOptions> configure)
    {
        return UseSiemens("Siemens", configure);
    }

    /// <summary>
    /// 配置内存模拟 PLC
    /// </summary>
    public PlcBuilder UseMemory(string name = "Memory")
    {
        var options = new PlcOptions
        {
            Name = name,
            Type = "Memory"
        };
        _clientConfigs.Add(options);
        return this;
    }

    /// <summary>
    /// 构建并注册所有 PLC 客户端
    /// </summary>
    public void Build()
    {
        foreach (var config in _clientConfigs)
        {
            var client = _factoryRegistry.Create(config);
            _provider.RegisterClient(config.Name, client);
        }

        // 注册提供者到 DI 容器
        _services.AddSingleton<IPlcProvider>(_provider);
    }
}

/// <summary>
/// 内存 PLC 客户端工厂
/// </summary>
public class MemoryPlcClientFactory : IPlcClientFactory
{
    public IPlcClient Create(PlcOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        return new MemoryPlcClient(options);
    }
}
