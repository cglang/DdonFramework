using VitrinRuntime.Desktop.Services;

namespace VitrinRuntime.Desktop.Stores;

public interface ILuaGroupStore
{
    List<LuaGroupConfig> GetAll();
    void Add(LuaGroupConfig config);
    bool Remove(string groupName);
    bool Contains(string groupName);
}
