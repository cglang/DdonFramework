using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Reflection;

public class DdonServiceInvoke : IDdonServiceInvoke
{
    private readonly IServiceProvider serviceProvider;

    public DdonServiceInvoke(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<dynamic?> IvnvokeAsync(string className, string methodName, params object[] parameter)
    {
        var classType = DdonType.GetTypeByName(className);
        var instance = serviceProvider.GetService(classType) ??
            throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

        var method = DdonType.GetMothodByName(instance.GetType(), methodName);
        return await DdonInvoke.InvokeAsync(instance, method, parameter);
    }

    public async Task<string?> IvnvokeGetJsonAsync<TOut>(string className, string methodName, params object[] parameter)
    {
        var result = await IvnvokeAsync(className, methodName, parameter);
        return JsonSerializer.Serialize(result);
    }
}