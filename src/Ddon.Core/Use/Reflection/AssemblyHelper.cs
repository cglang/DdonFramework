using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ddon.Core.Use.Reflection
{
    public class AssemblyHelper
    {
        /// <summary>
        /// 寻找实现类
        /// </summary>
        /// <param name="type">基类/接口</param>
        /// <param name="assembly">程序Assembly</param>
        /// <returns></returns>
        public static IEnumerable<Type> FindImplementType(Type type, params Assembly[] assembly)
        {
            return assembly.SelectMany(types => types.GetTypes())
                .Where(t => t.IsClass && t != type && type.IsAssignableFrom(t));
        }
    }
}
