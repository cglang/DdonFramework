using System.Threading.Tasks;
using Ddon.Core;
using Ddon.Core.Use.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationExtensions
{
    public static async Task InitApplicationInitializationAsync(this WebApplication app)
    {
        ObjectAccessor.WebApplication ??= app;

        foreach (var module in ModuleInfo.Instance.Modules)
        {
            var onApplicationInitializationMethod = module.GetType().GetMethod(nameof(ModuleCore.OnApplicationInitialization))!;
            var context = new ApplicationInitializationContext(app.Services);
            await DdonInvoke.InvokeAsync(module, onApplicationInitializationMethod, context);
        }

        ModuleInfo.Instance.Dispose();
    }
}
