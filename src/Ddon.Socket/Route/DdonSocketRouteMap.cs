using System.Collections.Generic;
namespace Ddon.Socket.Route
{
    internal static class DdonSocketRouteMap
    {
        private static readonly Dictionary<string, (string, string)> _routeMap = new();

        internal static void Init<TDdonSocketRouteMapLoadBase>() where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
        {
            var routeMap = new TDdonSocketRouteMapLoadBase();

            foreach (var route in routeMap.DdonSocketRoutes)
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

        public static (string, string)? Get(string route)
        {
            return _routeMap.ContainsKey(route) ? _routeMap[route] : null;
        }
    }
}
