using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Reflection
{
    public static class DdonInvoke
    {
        /// <summary>
        /// 通过类名和方法名执行方法中的代码
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="parameter">可反序列化为对象的参数</param>
        /// <returns></returns>
        /// <exception cref="Exception">当反序列化失败时或者执行多个参数的方法时引发异常</exception>
        public static async Task<dynamic?> InvokeAsync(string className, string methodName, string parameter)
        {
            var classType = DdonType.GetTypeByName(className);
            var method = DdonType.GetMothodByName(classType, methodName);
            var instance = Activator.CreateInstance(classType);
            return await InvokeAsync(instance, method, parameter);
        }

        /// <summary>
        /// 执行方法中的代码
        /// </summary>
        /// <param name="instance">类的对象</param>
        /// <param name="method">方法元数据</param>
        /// <param name="parameterText">可反序列化为对象的参数</param>
        /// <returns></returns>
        /// <exception cref="Exception">当反序列化失败时或者执行多个参数的方法时引发异常</exception>
        public static async Task<dynamic?> InvokeAsync(object? instance, MethodInfo method, string parameterText)
        {
            var parameters = method.GetParameters().AsEnumerable().ToList();
            if (!parameters.Any())
            {
                return await InvokeAsync(instance, method);
            }
            else if (parameters.Count() == 1)
            {
                var methodParameter = parameters.Select(x => x.ParameterType).ToList();
                if (string.IsNullOrEmpty(parameterText)) throw new Exception("参数不允许为空");

                if (methodParameter.First().Name == nameof(String))
                    return await InvokeAsync(instance, method, new object[] { parameterText });

                var methodParameterData = JsonSerializer.Deserialize(parameterText, methodParameter.First()) ?? throw new Exception($"序列化参数失败");
                return await InvokeAsync(instance, method, methodParameterData);
            }
            else
            {
                // TODO: 多个参数的支持 还有很多问题
                var elementParameter = JsonSerializer.Deserialize<JsonElement>(parameterText);

                List<object> methodParameteies = new();

                foreach (var parameter in parameters)
                {
                    var property = elementParameter.GetProperty(parameter.Name!);

                    switch (property.ValueKind)
                    {
                        case JsonValueKind.Number:
                            switch (parameter.ParameterType.Name)
                            {
                                case nameof(Int16):
                                    methodParameteies.Add(property.GetInt16());
                                    break;
                                case nameof(Int32):
                                    methodParameteies.Add(property.GetInt32());
                                    break;
                                case nameof(Int64):
                                    methodParameteies.Add(property.GetInt64());
                                    break;
                            }

                            break;
                        case JsonValueKind.String: methodParameteies.Add(property.GetString()!); break;
                        case JsonValueKind.Undefined:
                        case JsonValueKind.Object:
                        case JsonValueKind.Array:
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                        case JsonValueKind.Null:
                        default:
                            var data = JsonSerializer.Deserialize(property.GetString()!, parameter.ParameterType);
                            methodParameteies.Add(data!); break;
                    }
                }

                return await InvokeAsync(instance, method, methodParameteies.ToArray());
            }
        }

        /// <summary>
        /// 通过类名和方法名执行方法中的代码
        /// </summary>
        /// <param name="className">类型名称</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameter">参数列表</param>
        /// <returns></returns>
        public static async Task<dynamic?> InvokeAsync(string className, string methodName, params object[] parameter)
        {
            var classType = DdonType.GetTypeByName(className);
            var method = DdonType.GetMothodByName(classType, methodName);
            var instance = Activator.CreateInstance(classType);
            return await InvokeAsync(instance, method, parameter);
        }

        /// <summary>
        /// 执行方法中的代码
        /// </summary>
        /// <param name="instance">类的对象</param>
        /// <param name="method">方法元数据</param>
        /// <param name="parameter">参数列表</param>
        /// <returns>当方法返回值为 void 或 Task 时，返回 null</returns>
        public static async Task<dynamic?> InvokeAsync(object? instance, MethodInfo method, params object[] parameter)
        {
            dynamic? invokeResult = method.Invoke(instance, parameter);

            if (method.ReturnType.IsAssignableTo(typeof(Task)))
            {
                await invokeResult!;
                return null;
            }

            if (method.ReturnType.IsAssignableTo(typeof(Task<>)))
            {
                return await invokeResult!;
            }

            return invokeResult;
        }
    }
}
