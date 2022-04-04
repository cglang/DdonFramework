using Ddon.ConvenientSocket.Extra;
using Ddon.Socket.Connection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ddon.Socket
{
    public class DdonSocketInvoke
    {
        public static async Task<string> IvnvokeReturnJsonAsync(IServiceProvider services, Type classType, string methodName, string methodParameter,
            DdonSocketConnectionCore connection, DdonSocketHead head)
        {
            var returnData = await IvnvokeAsync(services, classType, methodName, methodParameter, connection, head);
            return JsonSerializer.Serialize(returnData);
        }

        public static async Task<dynamic?> IvnvokeAsync(IServiceProvider services, (Type, string) routeInfo, string methodParameter,
            DdonSocketConnectionCore connection, DdonSocketHead head)
        {
            return await IvnvokeAsync(services, routeInfo.Item1, routeInfo.Item2, methodParameter, connection, head);
        }

        public static async Task<dynamic?> IvnvokeAsync(IServiceProvider services, Type classType, string methodName, string methodParameterText,
            DdonSocketConnectionCore connection, DdonSocketHead head)
        {
            dynamic serviceObject = services.GetService(classType) ?? throw new Exception("找不到类型的对象");

            MethodInfo method = classType.GetMethods().FirstOrDefault(m => m.Name == methodName) ?? throw new Exception("找不到要执行的方法");
            IEnumerable<Type>? methodParameter = method?.GetParameters().Select(x => x.ParameterType);

            return await IvnvokeAsync(serviceObject, method, methodParameterText, connection, head);
        }

        private static async Task<dynamic?> IvnvokeAsync(dynamic serviceObject, MethodInfo method, string methodParameterText,
             DdonSocketConnectionCore connection, DdonSocketHead head)
        {
            IEnumerable<Type>? methodParameter = method?.GetParameters().Select(x => x.ParameterType);
            if (methodParameter is null || !methodParameter.Any())
            {
                return await IvnvokeAsync(serviceObject, method, Array.Empty<Type>(), connection, head);
            }
            else if (methodParameter.Count() == 1)
            {
                var methodParameterData = JsonSerializer.Deserialize(methodParameterText, methodParameter.First());
                List<object?> list = new() { methodParameterData };
                return await IvnvokeAsync(serviceObject, method, list.ToArray(), connection, head);
            }
            else
            {
                throw new Exception("暂时不支持执行多个参数的方法");
            }
        }

        private static async Task<dynamic?> IvnvokeAsync(dynamic serviceObject, MethodInfo method, object[] methodParameter,
            DdonSocketConnectionCore connection, DdonSocketHead head)
        {
            var serviceBase = (DdonSocketServiceBase)serviceObject;
            serviceBase.Connection = connection;
            serviceBase.Head = head;

            dynamic? methodReturn = method?.Invoke(serviceObject, methodParameter);
            if (methodReturn == null) return null;

            return IsAsyncMethod(method!) ? await methodReturn : methodReturn;
        }

        private static bool IsAsyncMethod(MethodInfo method)
        {
            Type attType = typeof(AsyncStateMachineAttribute);
            var attrib = (AsyncStateMachineAttribute?)method.GetCustomAttribute(attType);
            return attrib != null;
        }
    }
}
