using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Plugin;

public static class PluginFile
{
    public static Dictionary<string, IPlugin> Plugins => new();

    public static Dictionary<string, PluginInfo> PluginInfos => new();
}

public interface IPluginFileListenerServer
{
    void Start();

    void Stop();
}

public class PluginFileListenerServer : IPluginFileListenerServer
{
    private readonly FileSystemWatcher _watcher;

    public PluginFileListenerServer(string path)
    {
        _watcher = new FileSystemWatcher
        {
            Path = path,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.DirectoryName
        };
        //_watcher.IncludeSubdirectories = true;
        _watcher.Created += new FileSystemEventHandler(FileWatcherCreated);
        _watcher.Changed += new FileSystemEventHandler(FileWatcherChanged);
        _watcher.Deleted += new FileSystemEventHandler(FileWatcherDeleted);
        _watcher.Renamed += new RenamedEventHandler(FileWatcherRenamed);
    }


    public void Start()
    {
        // 开始监听
        _watcher.EnableRaisingEvents = true;
        Console.WriteLine(string.Format("==========【{0}】==========", "文件监控已经启动..."));
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
        Console.WriteLine(string.Format("==========【{0}】==========", "文件监控已经关闭"));
    }

    protected void FileWatcherCreated(object sender, FileSystemEventArgs e)
    {
        var dllFile = new FileInfo(e.FullPath);
        Assembly asm = Assembly.LoadFrom(dllFile.FullName);

        var baseType = typeof(IPlugin);
        var types = asm.GetTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();
        if (!types.Any())
        {
            Console.WriteLine("未继承插件接口");
            return;
        }

        foreach (var type in types)
        {
            if (PluginFile.Plugins.ContainsKey(type.FullName ?? type.Name))
            {
                Console.WriteLine("已经加载该插件");
                return;
            }
            if (!typeof(IPlugin).IsAssignableFrom(type))
            {
                Console.WriteLine($"{asm.ManifestModule.Name}未继承约定接口");
                return;
            }
            //dll实例化
            var instance = Activator.CreateInstance(type) as IPlugin ??
                throw new Exception();
            var protocolInfo = instance.GetPluginInformation();
            protocolInfo.LastTime = dllFile.LastWriteTime;

            Console.WriteLine($"插件名称：{protocolInfo.Name}");
            Console.WriteLine($"插件版本：{protocolInfo.Version}");
            Console.WriteLine($"插件作者：{protocolInfo.Author}");
            Console.WriteLine($"插件时间：{protocolInfo.LastTime}");

            PluginFile.Plugins.Add(type.FullName ?? type.Name, instance);
            PluginFile.PluginInfos.Add(type.FullName ?? type.Name, protocolInfo);
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
                Console.WriteLine(string.Format("==========【{0}】==========", "修改" + e.Name));
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
        var pluginName = e.Name.Split(".")[0];
        if (PluginFile.Plugins.ContainsKey(pluginName))
        {
            PluginFile.Plugins.Remove(pluginName);
            PluginFile.PluginInfos.Remove(pluginName);
            Console.WriteLine($"插件{e.Name}被移除");
        }
    }

    protected void FileWatcherRenamed(object sender, RenamedEventArgs e)
    {
        //TODO:暂时不做处理
        Console.WriteLine("重命名" + e.OldName + "->" + e.Name);
    }
}
