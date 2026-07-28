using Ddon.VitrinPLC.Models;

namespace VitrinRuntime.Desktop.Services;

/// <summary>点位配置</summary>
public sealed class TagConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string GroupId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public PlcDataType DataType { get; set; } = PlcDataType.Int32;
    public int StringLength { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
