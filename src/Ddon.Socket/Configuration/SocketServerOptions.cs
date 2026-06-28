using System.Net;

namespace Ddon.Socket.Configuration
{
    public class SocketServerOptions
    {
        public IPAddress Address { get; set; } = IPAddress.Any;

        public int Port { get; set; } = 8080;

        public int Backlog { get; set; } = 100;

        public bool NoDelay { get; set; } = true;

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendBufferSize { get; set; } = 4096;
    }
}
