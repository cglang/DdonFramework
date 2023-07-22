using System.Net;

namespace Ddon.Socket.Options
{
    public class SocketServerOption
    {
        public IPEndPoint IPEndPoint { get; set; }

        public SocketServerOption(string host, int port)
        {
            IPEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        public SocketServerOption(IPAddress host, int port)
        {
            IPEndPoint = new IPEndPoint(host, port);
        }
    }
}
