using Ddon.Core;
using Ddon.Core.Reflection;

namespace Ddon.Job
{
    public class DdonJobInvoke
    {
        public static async Task<dynamic?> IvnvokeAsync(string className, string methodName, string? parameterText = null)
        {
            var classType = DdonTypeHelper.GetTypeByName(className);
            var instance = DdonServiceProvider.GetServiceProvider().GetService(classType)
                ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var method = DdonTypeHelper.GetMothodByName(classType, methodName);
            if (string.IsNullOrEmpty(parameterText))
                parameterText = string.Empty;
            return await DdonInvokeHelper.InvokeAsync(instance, method, parameterText);
        }
    }
}
