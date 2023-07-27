using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Utility;

namespace Ddon.Socket.Session.Route
{
    internal static class SocketRouteMap
    {
        private static readonly Dictionary<string, (string, string)> _routeMap = new();

        internal static void Init()
        {
            var routes = new List<SocketRoute>();

            var baseType = typeof(SocketApiBase);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            foreach (var type in types)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var socketApi = method.GetCustomAttribute<SocketApiAttribute>();
                    if (socketApi == null) continue;

                    var routeText = $"{type.Name}::{socketApi.Template ?? method.Name}";
                    var route = new SocketRoute(routeText, type.Name, method.Name);
                    routes.Add(route);
                }
            }

            foreach (var route in routes)
            {
                Add(route.Route, route.ClassName, route.MethodName);
            }
        }

        private static void Add(string route, string className, string methodName)
        {
            if (!_routeMap.ContainsKey(route))
            {
                _routeMap.Add(route, (className, methodName));
            }
        }

        //public static (string, string)? Get(string route)
        //{
        //    return _routeMap.ContainsKey(route) ? _routeMap[route] : null;
        //}

        public static SocketEndPoint? Get(string route)
        {
            return _routeMap.ContainsKey(route) ? new(_routeMap[route].Item1, _routeMap[route].Item2) : null;
        }
    }
}
