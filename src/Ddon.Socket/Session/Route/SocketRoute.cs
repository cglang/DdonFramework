namespace Ddon.Socket.Session.Route
{
    public class SocketRoute
    {
        public SocketRoute(string route, string className, string methodName)
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
