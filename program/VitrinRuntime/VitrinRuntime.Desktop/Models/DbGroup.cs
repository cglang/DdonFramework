namespace VitrinRuntime.Desktop.Services;

/// <summary>DB 块分组</summary>
public sealed class DbGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PlcName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
