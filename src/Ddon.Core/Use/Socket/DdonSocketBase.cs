using System;
using System.IO;
using System.Net.Sockets;

namespace Ddon.Core.Use.Socket
{
    public abstract class DdonSocketBase : IDisposable
    {
        public Guid SocketId { get; }

        protected TcpClient TcpClient { get; }

        protected Stream Stream => TcpClient.GetStream();

        protected DdonSocketBase(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            SocketId = Guid.NewGuid();
        }

        public void Dispose()
        {
            TcpClient.Close();
            TcpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
