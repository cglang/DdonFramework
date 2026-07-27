using System.Collections.Generic;

namespace Ddon.LuaEngine
{
    public interface ILuaScriptManager
    {
        void LoadScriptsFromDirectory(string directoryPath, string groupName = null);

        void ReloadGroup(string groupName);

        void ReloadScript(string groupName, string fileName);

        void UnloadGroup(string groupName);

        void UnloadScript(string groupName, string fileName);

        IReadOnlyDictionary<string, LuaScriptGroup> GetAllGroups();

        LuaScriptGroup GetGroup(string groupName);

        bool ContainsGroup(string groupName);

        void EnableFileWatcher();

        void DisableFileWatcher();

        bool IsFileWatcherEnabled { get; }
    }
}
