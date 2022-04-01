namespace Ddon.ConvenientSocket
{
    public static class DdonSocketRouteMap
    {
        private static readonly Dictionary<string, (Type, string)> _routeMap = new();

        public static void Add<TType>(string route, string methodName)
        {
            if (!_routeMap.ContainsKey(route))
            {
                _routeMap.Add(route, (typeof(TType), methodName));
            }
        }

        public static (Type, string)? Get(string route)
        {
            return _routeMap.ContainsKey(route) ? _routeMap[route] : null;
        }
    }
}
