using System;
using System.Collections.Generic;
using Flow.Launcher.Core.Plugin;

namespace Ddon.Core.Use.Plugin
{
    public interface IPluginsLoader
    {
        void LoadPlugin(string dllPath);

        void ReloadPlugin(string dllPath);

        void RemovePlugin(string pluginName);

        void LoadPlugins(IEnumerable<string> dllPaths);
    }

    public class PluginsLoader : IPluginsLoader
    {
        public void LoadPlugin(string dllPath)
        {
            var assemblyLoader = new PluginAssemblyLoader(dllPath);
            var assembly = assemblyLoader.LoadAssemblyAndDependencies();

            var type = assemblyLoader.FromAssemblyGetTypeOfInterface(assembly, typeof(IPlugin));
            var plugin = Activator.CreateInstance(type) as IPlugin ?? throw new Exception("插件程序集加载错误");
            var protocolInfo = plugin.GetPluginInformation();

            PluginStorage.Plugins.Add(assembly.ManifestModule.Name, plugin);
            PluginStorage.PluginInfos.Add(assembly.ManifestModule.Name, protocolInfo);
        }

        public void ReloadPlugin(string dllPath)
        {
            var assemblyLoader = new PluginAssemblyLoader(dllPath);
            var assembly = assemblyLoader.LoadAssemblyAndDependencies();
            if (PluginStorage.PluginInfos.ContainsKey(assembly.ManifestModule.Name))
            {
                RemovePlugin(assembly.ManifestModule.Name);

                var type = assemblyLoader.FromAssemblyGetTypeOfInterface(assembly, typeof(IPlugin));
                var plugin = Activator.CreateInstance(type) as IPlugin ?? throw new Exception("插件程序集加载错误");
                var protocolInfo = plugin.GetPluginInformation();

                PluginStorage.Plugins.Add(assembly.ManifestModule.Name, plugin);
                PluginStorage.PluginInfos.Add(assembly.ManifestModule.Name, protocolInfo);
            }
        }

        public void LoadPlugins(IEnumerable<string> dllPaths)
        {
            foreach (var dllPath in dllPaths)
            {
                LoadPlugin(dllPath);
            }
        }

        public void RemovePlugin(string pluginName)
        {
            PluginStorage.Plugins[pluginName].Dispose();
            PluginStorage.Plugins.Remove(pluginName);
            PluginStorage.PluginInfos.Remove(pluginName);
        }
    }
}
