using System;

namespace Ddon.Core.Use.Plugin;

public class PluginInfo
{
    public PluginInfo(string name, string version, string author, DateTime lastTime)
    {
        Name = name;
        Version = version;
        Author = author;
        LastTime = lastTime;
    }

    public string Name { set; get; }

    public string Version { set; get; }

    public string Author { set; get; }

    public DateTime LastTime { set; get; }
}
