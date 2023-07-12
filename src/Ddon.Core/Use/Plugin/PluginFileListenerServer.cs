using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ddon.Core.Use.Plugin;

public class PluginFileListenerServer : IPluginFileListenerServer
{
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger _logger;
    private readonly IPluginsLoader _pluginsLoader;

    public PluginFileListenerServer(ILogger logger, IOptions<PluginOptions> options, IPluginsLoader pluginsLoader)
    {
        Directory.CreateDirectory(options.Value.PluginPath);

        _watcher = new FileSystemWatcher
        {
            Path = options.Value.PluginPath,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.DirectoryName
        };

        _watcher.Created += new FileSystemEventHandler(FileWatcherCreated);
        _watcher.Changed += new FileSystemEventHandler(FileWatcherChanged);
        _watcher.Deleted += new FileSystemEventHandler(FileWatcherDeleted);

        _logger = logger;
        _pluginsLoader = pluginsLoader;
    }

    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("插件监控已经启动");
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
        _logger.LogInformation("插件监控已经关闭");
    }

    protected void FileWatcherCreated(object sender, FileSystemEventArgs e)
    {
        var dllFile = new FileInfo(e.FullPath);
        if (dllFile.Extension.ToUpper() == "DLL")
        {
            _pluginsLoader.LoadPlugin(dllFile.FullName);
        }
    }

    protected void FileWatcherChanged(object sender, FileSystemEventArgs e)
    {
        var dllFile = new FileInfo(e.FullPath);
        if (dllFile.Extension.ToUpper() == "DLL")
        {
            _pluginsLoader.ReloadPlugin(dllFile.FullName);
        }
    }

    protected void FileWatcherDeleted(object sender, FileSystemEventArgs e)
    {
        var dllFile = new FileInfo(e.FullPath);
        if (dllFile.Extension.ToUpper() == "DLL")
        {
            _pluginsLoader.RemovePlugin(dllFile.Name);
        }
    }
}
