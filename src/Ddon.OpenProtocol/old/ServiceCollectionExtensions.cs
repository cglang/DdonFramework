using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppWinUI.DdonOPClient;
using Ddon.OpenProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenProtocol.Events;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.IOInterface;

namespace OpenProtocol.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenProtocolClient(
    this IServiceCollection services,
    Action<OpenProtocolClientOptions>? configure = null)
    {
        // ── Options ───────────────────────────────────────────────
        var options = new OpenProtocolClientOptions();
        configure?.Invoke(options);
        services.TryAddSingleton(options);

        // ── MidInterpreter ────────────────────────────────────────
        services.TryAddSingleton(_ => new MidInterpreter()
            // 注册自定义 MID（库未实现的）
            .UseCustomMessage(new Dictionary<int, Type>
            {
                //{ Mid0900.MID, typeof(Mid0900) },  // 曲线数据
                //{ Mid0901.MID, typeof(Mid0901) },  // 曲线结束
                { Mid0224.MID, typeof(Mid0224) },  //
            })
            .UseAllMessages()
        );

        // ── EventBus ──────────────────────────────────────────────
        services.TryAddSingleton<OpenProtocolEventBus>();

        // ── Client ────────────────────────────────────────────────
        services.TryAddSingleton<OpenProtocolClient>();
        services.TryAddSingleton<IOpenProtocolClient>(
            sp => sp.GetRequiredService<OpenProtocolClient>());

        return services;
    }

    // ── 命名客户端（链式调用）────────────────────────────────────────
    public static IServiceCollection AddNamedOpenProtocolClient(
        this IServiceCollection services,
        string name,
        Action<OpenProtocolClientOptions> configure)
    {
        var opts = new OpenProtocolClientOptions { Name = name };
        configure(opts);
        return services.AddNamedOpenProtocolClient(opts);
    }

    private static IServiceCollection AddNamedOpenProtocolClient(
        this IServiceCollection services,
        OpenProtocolClientOptions opts)
    {
        // 每次调用追加一条 options（同一接口允许多注册）
        services.AddSingleton(opts);

        // Factory 和接口只注册一次
        services.TryAddSingleton<OpenProtocolClientFactory>();
        services.TryAddSingleton<IOpenProtocolClientFactory>(
            sp => sp.GetRequiredService<OpenProtocolClientFactory>());

        return services;
    }

    // ── 启动时批量连接 ─────────────────────────────────────────────
    public static async Task UseOpenProtocolClientsAsync(
        this IServiceProvider services,
        CancellationToken ct = default)
    {
        var factory = services.GetRequiredService<IOpenProtocolClientFactory>();
        var tasks = factory
            .GetRegisteredNames()
            .Select(name => factory.GetClient(name).ConnectAsync(ct));
        await Task.WhenAll(tasks);
    }
}
