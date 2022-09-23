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

        public DdonSocketBase(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            SocketId = Guid.NewGuid();
            ConsecutiveReadStream();
        }

        protected abstract void ConsecutiveReadStream();

        public void Dispose()
        {
            if (TcpClient is not null)
            {
                TcpClient.Dispose();
                //TcpClient.Close();
            }
            GC.SuppressFinalize(this);
        }
    }
}
