namespace Ddon.OpcUaServer.Server;

/// <summary>
/// OPC UA Server 核心接口，提供启停控制、节点管理器和状态通知。
/// </summary>
public interface IVitrinUaServer : IAsyncDisposable
{
    /// <summary>Server 当前是否正在运行。</summary>
    bool IsRunning { get; }

    /// <summary>Server 绑定的端点地址。</summary>
    string EndpointUrl { get; }

    /// <summary>节点管理器（持有地址空间所有节点）。</summary>
    NodeManager.IVitrinNodeManager NodeManager { get; }

    /// <summary>启动 OPC UA Server。</summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>停止 OPC UA Server。</summary>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>Server 状态变化事件。</summary>
    event EventHandler<ServerStatusChangedEventArgs>? StatusChanged;
}

/// <summary>Server 状态变化事件参数。</summary>
public sealed class ServerStatusChangedEventArgs : EventArgs
{
    /// <summary>是否正在运行。</summary>
    public bool IsRunning { get; init; }

    /// <summary>状态消息。</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>发生时间。</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
