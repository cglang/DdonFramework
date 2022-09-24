using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Reflection;

public class DdonServiceInvoke : IDdonServiceInvoke
{
    private readonly IServiceProvider _serviceProvider;

    public DdonServiceInvoke(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<dynamic?> InvokeAsync(string className, string methodName, params object[] parameter)
    {
        var (instance, method) = GetInVokeInfo(className, methodName);
        return await DdonInvoke.InvokeAsync(instance, method, parameter);
    }

    public async Task<string?> InvokeGetJsonAsync<TOut>(string className, string methodName, params object[] parameter)
    {
        var result = await InvokeAsync(className, methodName, parameter);
        return JsonSerializer.Serialize(result);
    }

    public async Task<dynamic?> InvokeAsync(string className, string methodName, string parameter)
    {
        var (instance, method) = GetInVokeInfo(className, methodName);
        return await DdonInvoke.InvokeAsync(instance, method, parameter);
    }

    private (object, MethodInfo) GetInVokeInfo(string className, string methodName)
    {
        var classType = DdonType.GetTypeByName(className);
        var instance = _serviceProvider.GetService(classType) ??
            throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

        var method = DdonType.GetMothodByName(instance.GetType(), methodName);

        return (instance, method);
    }
}