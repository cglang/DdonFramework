namespace Ddon.Socket.Session.Model
{
    public class SocketEndPoint
    {
        public SocketEndPoint(string className, string methodName)
        {
            ClassName = className;
            MethodName = methodName;
        }

        public string ClassName { get; set; }

        public string MethodName { get; set; }
    }
}
