using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Models;

namespace Ddon.UniPLC.Clients.Siemens;

/// <summary>
/// Siemens PLC 客户端配置选项
/// </summary>
public class SiemensPlcOptions : PlcOptions
{
    /// <summary>
    /// Rack 号
    /// </summary>
    public int Rack { get; set; } = 0;

    /// <summary>
    /// Slot 号
    /// </summary>
    public int Slot { get; set; } = 1;

    /// <summary>
    /// DB 块大小配置（块号 -> 大小）
    /// </summary>
    public Dictionary<int, int> DbBlockSizes { get; set; } = new();

    public SiemensPlcOptions()
    {
        Type = "Siemens";
        Port = 102;
    }
}
