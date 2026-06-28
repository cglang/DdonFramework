using System.Net;

namespace Ddon.Socket.Configuration
{
    public class SocketClientOptions
    {
        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 8080;

        public int ConnectTimeout { get; set; } = 5000;

        public int SendTimeout { get; set; } = 5000;

        public int ReceiveTimeout { get; set; } = 5000;

        public bool NoDelay { get; set; } = true;

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendBufferSize { get; set; } = 4096;
    }
}
