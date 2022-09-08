using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Reflection;

public class DdonServiceInvoke : IDdonServiceInvoke
{
    private readonly IServiceProvider serviceProvider;

    public DdonServiceInvoke(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<dynamic?> IvnvokeAsync<TClass>(string methodName, params object[] parameter) where TClass : class
    {
        var instance = serviceProvider.GetService<TClass>() ??
            throw new Exception($"从 [ServiceProvider] 中找不到 [{nameof(TClass)}] 类型的对象");

        var method = DdonType.GetMothodByName(instance.GetType(), methodName);
        return await DdonInvoke.InvokeAsync(instance, method, parameter);
    }
}