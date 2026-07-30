namespace Ddon.OpcUaServer.Server;

/// <summary>
/// OPC UA Server 配置选项。
/// </summary>
public sealed class VitrinUaServerOptions
{
    /// <summary>OPC UA Server 端点地址，默认 "opc.tcp://localhost:4840"</summary>
    public string EndpointUrl { get; set; } = "opc.tcp://localhost:4840";

    /// <summary>Server 名称，显示在 UA Client 的发现列表中。</summary>
    public string ServerName { get; set; } = "VitrinRuntime";

    /// <summary>证书存储目录，默认在 AppData 下。</summary>
    public string CertificateStorePath { get; set; } = string.Empty;

    /// <summary>是否允许外部客户端匿名连接。</summary>
    public bool AllowAnonymous { get; set; } = true;

    /// <summary>最大会话数。</summary>
    public uint MaxSessionCount { get; set; } = 100;
}
