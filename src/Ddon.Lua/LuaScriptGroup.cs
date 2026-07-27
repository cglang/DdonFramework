using System;
using System.Collections.Generic;
using NLua;

namespace Ddon.LuaEngine
{
    public class LuaScriptGroup : IDisposable
    {
        public string GroupName { get; }

        public string DirectoryPath { get; }

        public Lua LuaVm { get; internal set; }

        public Dictionary<string, LuaScriptFile> Scripts { get; }

        public LuaScriptGroup(string groupName, string directoryPath, Lua luaVm)
        {
            GroupName = groupName;
            DirectoryPath = directoryPath;
            LuaVm = luaVm;
            Scripts = new Dictionary<string, LuaScriptFile>(StringComparer.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            LuaVm?.Dispose();
            Scripts.Clear();
        }
    }

    public class LuaScriptFile
    {
        public string FilePath { get; set; }

        public string FileName { get; set; }

        public DateTime LastWriteTime { get; set; }

        public bool IsLoaded { get; set; }
    }
}
