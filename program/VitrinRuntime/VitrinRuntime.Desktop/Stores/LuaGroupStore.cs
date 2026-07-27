namespace VitrinRuntime.Desktop.Stores;

public sealed class LuaGroupConfig
{
    public string GroupName { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public interface ILuaGroupStore
{
    List<LuaGroupConfig> GetAll();
    void Add(LuaGroupConfig config);
    bool Remove(string groupName);
    bool Contains(string groupName);
}
