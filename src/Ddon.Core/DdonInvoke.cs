using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Core
{
    public class DdonInvoke
    {
        public static async Task<string> IvnvokeReturnJsonAsync(IServiceProvider services, Type classType, string methodName, string methodParameterText)
        {
            var returnData = await IvnvokeAsync(services, classType, methodName, methodParameterText);
            return JsonSerializer.Serialize(returnData);
        }

        public static async Task<dynamic?> IvnvokeAsync(IServiceProvider services, Type classType, string methodName, string methodParameterText)
        {
            dynamic serviceObject = services.GetService(classType) ?? throw new Exception("找不到类型的对象");

            MethodInfo method = classType.GetMethods().FirstOrDefault(m => m.Name == methodName) ?? throw new Exception("找不到要执行的方法");
            IEnumerable<Type>? methodParameter = method?.GetParameters().Select(x => x.ParameterType);

            return await IvnvokeAsync(serviceObject, method, methodParameterText);
        }

        private static async Task<dynamic?> IvnvokeAsync(dynamic serviceObject, MethodInfo method, string methodParameterText)
        {
            IEnumerable<Type>? methodParameter = method?.GetParameters().Select(x => x.ParameterType);
            if (methodParameter is null || !methodParameter.Any())
            {
                return await IvnvokeAsync(serviceObject, method, Array.Empty<Type>());
            }
            else if (methodParameter.Count() == 1)
            {
                var methodParameterData = JsonSerializer.Deserialize(methodParameterText, methodParameter.First());
                List<object?> list = new() { methodParameterData };
                return await IvnvokeAsync(serviceObject, method, list.ToArray());
            }
            else
            {
                throw new Exception("暂时不支持执行多个参数的方法");
            }
        }

        private static async Task<dynamic?> IvnvokeAsync(dynamic serviceObject, MethodInfo method, object[] methodParameter)
        {
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