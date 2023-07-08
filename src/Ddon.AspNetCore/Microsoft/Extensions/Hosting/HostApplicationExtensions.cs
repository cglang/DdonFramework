using System.Threading.Tasks;
using Ddon.Core;
using Ddon.Core.Use.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationExtensions
{
    public static async Task InitApplicationInitializationAsync(this IApplicationBuilder app)
    {
        ObjectAccessor.ApplicationBuilder ??= app;

        foreach (var module in ModuleInfo.Instance.Modules)
        {
            var onApplicationInitializationMethod = module.GetType().GetMethod(nameof(ModuleCore.OnApplicationInitialization))!;
            var context = new ApplicationInitializationContext(app.ApplicationServices);
            await DdonInvoke.InvokeAsync(module, onApplicationInitializationMethod, context);
        }

        ModuleInfo.Instance.Dispose();
    }
}
