using Ddon.Core.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Socket
{
    public abstract class DdonSocketBase : IDisposable
    {
        protected Func<DdonSocketCore, byte[], Task>? _byteHandler;
        protected Func<DdonSocketCore, string, Task>? _stringHandler;
        protected Func<DdonSocketCore, DdonSocketException, Task>? _exceptionHandler;

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
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
