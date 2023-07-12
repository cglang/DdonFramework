using System;

namespace Ddon.Core.Use.Plugin;

public class PluginInfo
{
    public PluginInfo(string name, string version, string author)
    {
        Name = name;
        Version = version;
        Author = author;
        LoadTime = DateTime.UtcNow;
    }

    public string Name { set; get; }

    public string Version { set; get; }

    public string Author { set; get; }

    public DateTime LoadTime { set; get; }
}
