using System.Collections.Generic;

namespace Ddon.Core.Use.Plugin;

public static class PluginStorage
{
    public static Dictionary<string, IPlugin> Plugins = new();

    public static Dictionary<string, PluginInfo> PluginInfos = new();
}
