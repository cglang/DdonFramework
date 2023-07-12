using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ddon.Core.Services.IdWorker.Guids;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ddon.Core.Use.Plugin;

public static class PluginFile
{
    public static Dictionary<string, IPlugin> Plugins => new();

    public static Dictionary<string, PluginInfo> PluginInfos => new();
}

public class PluginFileListenerServer : IPluginFileListenerServer
{
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger _logger;

    public PluginFileListenerServer(ILogger logger, IOptions<PluginOptions> options)
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
        _watcher.Renamed += new RenamedEventHandler(FileWatcherRenamed);
        _logger = logger;
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
        var asm = Assembly.LoadFrom(dllFile.FullName);

        _logger.LogInformation($"{dllFile.FullName}=={dllFile.Name}=={asm.FullName}=={asm}");

        var baseType = typeof(IPlugin);
        var types = asm.GetTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type));

        foreach (var type in types)
        {
            if (PluginFile.Plugins.ContainsKey(type.Name))
            {
                Console.WriteLine("已经加载该插件");
                return;
            }

            var plugin = Activator.CreateInstance(type) as IPlugin ?? throw new Exception();

            var protocolInfo = plugin.GetPluginInformation();
            protocolInfo.LastTime = dllFile.LastWriteTime;

            Console.WriteLine($"插件名称：{protocolInfo.Name}");
            Console.WriteLine($"插件版本：{protocolInfo.Version}");
            Console.WriteLine($"插件作者：{protocolInfo.Author}");
            Console.WriteLine($"插件时间：{protocolInfo.LastTime}");

            PluginFile.Plugins.Add(type.Name, plugin);
            PluginFile.PluginInfos.Add(type.Name, protocolInfo);
        }
    }

    protected void FileWatcherChanged(object sender, FileSystemEventArgs e)
    {
        var pluginName = e.Name.Split(".")[0];
        var dll = new FileInfo(e.FullPath);
        // 替换插件
        if (PluginFile.PluginInfos.ContainsKey(pluginName))
        {
            // 修改时间不一致,说明是新的插件
            if (PluginFile.PluginInfos[pluginName].LastTime != dll.LastWriteTime)
            {
                _logger.LogInformation("==========【{0}】==========", "修改" + e.Name);
                // 更新
                var fileData = File.ReadAllBytes(e.FullPath);
                var asm = Assembly.Load(fileData);
                var manifestModuleName = asm.ManifestModule.ScopeName;
                var classLibrayName = manifestModuleName.Remove(manifestModuleName.LastIndexOf("."), manifestModuleName.Length - manifestModuleName.LastIndexOf("."));
                var type = asm.GetType("Plugin_Test" + "." + classLibrayName);
                if (!typeof(IPlugin).IsAssignableFrom(type))
                {
                    Console.WriteLine($"{asm.ManifestModule.Name}未继承约定接口");
                    return;
                }
                var instance = Activator.CreateInstance(type) as IPlugin;
                var protocolInfo = instance.GetPluginInformation();
                protocolInfo.LastTime = dll.LastWriteTime;

                Console.WriteLine($"插件名称：{protocolInfo.Name}");
                Console.WriteLine($"插件版本：{protocolInfo.Version}");
                Console.WriteLine($"插件作者：{protocolInfo.Author}");
                Console.WriteLine($"插件时间：{protocolInfo.LastTime}");

                PluginFile.Plugins[classLibrayName] = instance;
                PluginFile.PluginInfos[classLibrayName] = protocolInfo;

                // 避免多次触发
                _watcher.EnableRaisingEvents = false;
                _watcher.EnableRaisingEvents = true;
            }
        }
    }

    protected void FileWatcherDeleted(object sender, FileSystemEventArgs e)
    {
        if (e.Name is not null && PluginFile.Plugins.ContainsKey(e.Name))
        {
            PluginFile.Plugins.Remove(e.Name);
            PluginFile.PluginInfos.Remove(e.Name);
            Console.WriteLine($"插件{e.Name}被移除");
        }
    }

    protected void FileWatcherRenamed(object sender, RenamedEventArgs e)
    {
        //TODO:暂时不做处理
        Console.WriteLine("重命名" + e.OldName + "->" + e.Name);
    }
}
