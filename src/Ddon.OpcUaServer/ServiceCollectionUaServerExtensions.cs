using Ddon.OpcUaServer.NodeManager;
using Ddon.OpcUaServer.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ddon.OpcUaServer;

/// <summary>
/// OPC UA Server 的 DI 注册扩展方法。
/// </summary>
public static class ServiceCollectionUaServerExtensions
{
    /// <summary>
    /// 注册 OPC UA Server 服务到 DI 容器。
    /// 各设备模块通过 <c>services.AddSingleton&lt;INodeProvider, XxxProvider&gt;()</c>
    /// 自行注册节点提供者。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configure">可选配置委托。</param>
    public static IServiceCollection AddVitrinUaServer(
        this IServiceCollection services,
        Action<VitrinUaServerOptions>? configure = null)
    {
        // 注册配置
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<VitrinUaServerOptions>(_ => { });
        }

        // 注册 Server 单例（包含 IVitrinNodeManager 节点管理器）
        services.TryAddSingleton<IVitrinUaServer, VitrinUaServer>();

        // 注册节点管理器（从 Server 实例获取）
        services.TryAddSingleton<IVitrinNodeManager>(sp =>
            sp.GetRequiredService<IVitrinUaServer>().NodeManager);

        return services;
    }
}
