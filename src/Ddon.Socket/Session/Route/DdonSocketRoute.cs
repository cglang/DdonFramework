namespace Ddon.Socket.Session.Route
{
    public class DdonSocketRoute
    {
        public DdonSocketRoute(string route, string className, string methodName)
        {
            Route = route;
            ClassName = className;
            MethodName = methodName;
        }

        public string Route { get; set; }

        public string ClassName { get; set; }

        public string MethodName { get; set; }
    }
}
