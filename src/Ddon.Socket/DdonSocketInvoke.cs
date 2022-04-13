using Ddon.ConvenientSocket.Extra;
using Ddon.Core;
using System.Text.Json;

namespace Ddon.Socket
{
    public class DdonSocketInvoke
    {
        public static async Task<string> IvnvokeReturnJsonAsync(
            IServiceProvider services,
            string className,
            string methodName,
            string methodParameter,
            DdonSocketConnectionCore connection,
            DdonSocketHead head)
        {
            var returnData = await IvnvokeAsync(services, className, methodName, methodParameter, connection, head);
            return JsonSerializer.Serialize(returnData);
        }

        public static async Task<dynamic?> IvnvokeAsync(
            IServiceProvider services,
            string className,
            string methodName,
            string parameter,
            DdonSocketConnectionCore connection,
            DdonSocketHead head)
        {
            var classType = DdonTypeHelper.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (DdonSocketControllerBase) instance;
            ddonSocketService.Connection = connection;
            ddonSocketService.Head = head;

            var method = DdonTypeHelper.GetMothodByName(classType, methodName);
            return await DdonInvokeHelper.InvokeAsync(instance, method, parameter);
        }

        public static async Task<dynamic?> IvnvokeAsync<T>(
            IServiceProvider services,
            string className,
            string methodName,
            T parameter,
            DdonSocketConnectionCore connection,
            DdonSocketHead head) where T : notnull
        {
            var classType = DdonTypeHelper.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (DdonSocketControllerBase)instance;
            ddonSocketService.Connection = connection;
            ddonSocketService.Head = head;

            var method = DdonTypeHelper.GetMothodByName(classType, methodName);
            return await DdonInvokeHelper.InvokeAsync(instance, method, parameter);
        }
    }
}
