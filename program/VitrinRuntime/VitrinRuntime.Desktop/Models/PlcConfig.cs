namespace VitrinRuntime.Desktop.Services;

/// <summary>PLC 连接配置</summary>
public sealed class PlcConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string PlcType { get; set; } = "Siemens";
    public string Ip { get; set; } = "192.168.0.0";
    public int Port { get; set; } = 102;
    public int ScanInterval { get; set; } = 200;
    public bool AutoConnect { get; set; }
    public bool IsConnected { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastConnectedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> ConnectionOptions { get; set; } = new();
}
