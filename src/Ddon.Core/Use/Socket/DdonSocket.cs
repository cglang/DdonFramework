namespace Ddon.Core.Use.Socket
{
    public static class DdonSocket
    {
        public static DdonSocketServer CreateServer(string host, int port) => new(host, port);

        public static DdonSocketCore CreateClient(string serverhost, int port) => new(serverhost, port);
    }
}
