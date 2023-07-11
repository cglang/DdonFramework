using System;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Plugin;

public interface IPlugin : IDisposable
{
    PluginInfo GetPluginInformation();

    Task LoadAsync();
}
