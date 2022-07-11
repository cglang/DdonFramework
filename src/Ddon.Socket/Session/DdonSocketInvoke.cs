using Ddon.Core.Use.Reflection;
using Ddon.Socket.Session.Model;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    public class DdonSocketInvoke
    {
        public static async Task<string> IvnvokeReturnJsonAsync(
            IServiceProvider services,
            string className,
            string methodName,
            string methodParameter,
            DdonSocketSession connection,
            DdonSocketRequest head)
        {
            var returnData = await IvnvokeAsync(services, className, methodName, methodParameter, connection, head);
            return JsonSerializer.Serialize(returnData);
        }

        public static async Task<dynamic?> IvnvokeAsync(
            IServiceProvider services,
            string className,
            string methodName,
            string parameter,
            DdonSocketSession connection,
            DdonSocketRequest head)
        {
            var classType = DdonType.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (DdonSocketApiCore)instance;
            ddonSocketService.Session = connection;
            ddonSocketService.Head = head;

            var method = DdonType.GetMothodByName(classType, methodName);
            return await DdonInvoke.InvokeAsync(instance, method, parameter);
        }

        public static async Task<dynamic?> IvnvokeAsync<T>(
            IServiceProvider services,
            string className,
            string methodName,
            T parameter,
            DdonSocketSession connection,
            DdonSocketRequest head) where T : notnull
        {
            var classType = DdonType.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (DdonSocketApiCore)instance;
            ddonSocketService.Session = connection;
            ddonSocketService.Head = head;

            var method = DdonType.GetMothodByName(classType, methodName);
            return await DdonInvoke.InvokeAsync(instance, method, parameter);
        }
    }
}
