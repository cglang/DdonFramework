namespace VitrinRuntime.Desktop.Services;

public sealed class LuaGroupConfig
{
    public string GroupName { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}