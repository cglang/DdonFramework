namespace Ddon.Core.Use.Plugin
{
    public class PluginOptions
    {
        /// <summary>
        /// 插件所在路径
        /// </summary>
        public string PluginPath { get; set; }

        public PluginOptions(string pluginPath)
        {
            PluginPath = pluginPath;
        }
    }
}
