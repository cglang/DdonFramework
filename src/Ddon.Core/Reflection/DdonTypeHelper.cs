using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ddon.Core
{
    public static class DdonTypeHelper
    {
        private readonly static IEnumerable<Type> Types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes());

        /// <summary>
        /// 通过类型名称寻找类型
        /// </summary>
        /// <param name="className">类型名称</param>
        /// <returns></returns>
        /// <exception cref="Exception">通过名称找不到类型时引发异常</exception>
        public static Type GetTypeByName(string className)
        {
            return Types.FirstOrDefault(type => type.Name.Equals(className)) ?? throw new Exception($"找不到名为[{className}]的类型");
        }

        /// <summary>
        /// 通过类型名称寻找类型
        /// </summary>
        /// <typeparam name="T">基类约束</typeparam>
        /// <param name="className">类型名称</param>
        /// <returns></returns>
        /// <exception cref="Exception">通过名称找不到类型时引发异常</exception>
        public static Type GetTypeByName<T>(string className)
        {
            var baseType = typeof(T);
            var type = Types.FirstOrDefault(type => type.Name.Equals(className, StringComparison.Ordinal) && baseType.IsAssignableFrom(type))
                ?? throw new Exception($"找不到名为[{className}]的类型");
            return type;
        }

        /// <summary>
        /// 获取一个方法的元数据
        /// </summary>
        /// <param name="classType">方法所在类的类型</param>
        /// <param name="methodName">方法名称</param>
        /// <returns></returns>
        /// <exception cref="Exception">类型内找不到指定的方法时引发异常</exception>
        public static MethodInfo GetMothodByName(Type classType, string methodName)
        {
            var method = classType.GetMethods().FirstOrDefault(m => m.Name == methodName)
                ?? throw new Exception($"[{nameof(classType)}]内找不到要名为[{methodName}]的方法");
            return method;
        }
    }
}
