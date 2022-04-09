using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Core
{
    public class DdonInvokeHelper
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
            var classType = DdonTypeHelper.GetTypeByName(className);
            var method = DdonTypeHelper.GetMothodByName(classType, methodName);
            var instance = Activator.CreateInstance(classType);
            return await InvokeAsync(instance, method, parameter);
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
            var classType = DdonTypeHelper.GetTypeByName(className);
            var method = DdonTypeHelper.GetMothodByName(classType, methodName);
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
            IEnumerable<Type>? methodParameter = method.GetParameters().Select(x => x.ParameterType);
            if (methodParameter is null || !methodParameter.Any())
            {
                return await InvokeAsync(instance, method);
            }
            else if (methodParameter.Count() == 1)
            {
                var methodParameterData = JsonSerializer.Deserialize(parameterText, methodParameter.First())
                    ?? throw new Exception($"序列化参数失败");
                return await InvokeAsync(instance, method, methodParameterData);
            }

            throw new Exception("暂不支持执行多个参数的方法");
        }

        /// <summary>
        /// 执行方法中的代码
        /// </summary>
        /// <param name="instance">类的对象</param>
        /// <param name="method">方法元数据</param>
        /// <param name="parameter">参数列表</param>
        /// <returns>当方法返回值为 viod 或 Task 时，返回 null</returns>
        public static async Task<dynamic?> InvokeAsync(object? instance, MethodInfo method, params object[] parameter)
        {
            dynamic? methodReturn = method.Invoke(instance, parameter);

            if (IsAsyncMethod(method))
            {
                methodReturn = method.ReturnType == typeof(Task) ? null : await methodReturn;
            }

            return methodReturn;
        }

        /// <summary>
        /// 是否为异步方法
        /// </summary>
        private static bool IsAsyncMethod(MethodInfo method)
        {
            var atttype = typeof(AsyncStateMachineAttribute);
            var attrib = (AsyncStateMachineAttribute?)method.GetCustomAttribute(atttype);
            return attrib != null;
        }
    }

    public class DdonInvokeForServiceHelper
    {
        public static async Task<dynamic?> InvokeAsync(IServiceProvider services, string className, string methodName, string parameterText)
        {
            var classType = DdonTypeHelper.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var method = DdonTypeHelper.GetMothodByName(classType, methodName);
            return await DdonInvokeHelper.InvokeAsync(instance, method, parameterText);
        }

        /// <summary>
        /// 从容器中获取类的对象并执行方法中的代码
        /// </summary>
        /// <param name="services">容器对象</param>
        /// <param name="className">类型名称</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameter">参数列表</param>
        /// <returns></returns>
        public static async Task<dynamic?> InvokeAsync(IServiceProvider services, string className, string methodName, params object[] parameter)
        {
            var classType = DdonTypeHelper.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");
            
            var method = DdonTypeHelper.GetMothodByName(classType, methodName);
            return await DdonInvokeHelper.InvokeAsync(instance, method, parameter);
        }

        ///// <summary>
        ///// 从容器中获取类的对象并执行方法中的代码
        ///// </summary>
        ///// <param name="services">容器对象</param>
        ///// <param name="classType">对象的类型</param>
        ///// <param name="methodName">方法名</param>
        ///// <param name="parameter">参数列表</param>
        ///// <returns></returns>
        ///// <exception cref="Exception">从容器中找不到对象时引发异常</exception>
        //private static async Task<dynamic?> IvnvokeAsync(IServiceProvider services, Type classType, MethodInfo method, string parameter)
        //{
        //    var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");
        //    return await DdonInvokeHelper.IvnvokeAsync(instance, method, parameter);
        //}
    }
}
