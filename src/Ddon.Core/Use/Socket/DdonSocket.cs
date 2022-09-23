namespace Ddon.Core.Use.Socket
{
    public class DdonSocket
    {
        public static DdonSocketServer CreateServer(string host, int port) => new(host, port);

        public static DdonSocketCore CreateClient(string serverhost, int port) => new(serverhost, port);
    }
}
