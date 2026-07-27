using System.Collections.Generic;
using NLua;

namespace Ddon.LuaEngine
{
    public interface ILuaVmManager
    {
        IReadOnlyDictionary<string, Lua> GetAllVms();

        Lua GetVm(string groupName);

        bool ContainsVm(string groupName);

        Lua AddVm(string groupName);

        void AddVm(string groupName, Lua vm);

        bool RemoveVm(string groupName);

        void SetVm(string groupName, Lua vm);

        void ClearAllVms();

        IReadOnlyList<string> GetFunctionNames(string groupName);
    }
}
