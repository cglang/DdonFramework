namespace Ddon.LuaEngine
{
    public class LuaOptions
    {
        public bool EnableFileWatcher { get; set; } = true;

        public int FileWatcherDebounceMilliseconds { get; set; } = 300;

        public string ScriptRootPath { get; set; } = string.Empty;
    }
}
