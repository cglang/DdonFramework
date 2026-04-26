namespace Ddon.UniPLC.Models;

/// <summary>
/// PLC 配置选项
/// </summary>
public class PlcOptions
{
    /// <summary>
    /// 客户端名称
    /// </summary>
    public string Name { get; set; } = "Default";

    /// <summary>
    /// PLC 类型
    /// </summary>
    public string Type { get; set; } = "Siemens";

    /// <summary>
    /// IP 地址
    /// </summary>
    public string Ip { get; set; } = "127.0.0.1";

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; } = 102;

    /// <summary>
    /// 连接超时时间（毫秒）
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// 读写超时时间（毫秒）
    /// </summary>
    public int OperationTimeout { get; set; } = 5000;

    /// <summary>
    /// 自动重连间隔（秒）
    /// </summary>
    public int ReconnectInterval { get; set; } = 3;

    /// <summary>
    /// 自定义选项字典
    /// </summary>
    public Dictionary<string, object> ExtendedOptions { get; set; } = new();
}
