using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Socket.Core
{
    public abstract class SocketSessionHandlerBase : ISocketSessionBind
    {
        protected Func<SocketSession, Memory<byte>, Task>? ByteHandler;
        protected Func<SocketSession, string, Task>? StringHandler;
        protected Func<SocketSession, Exceptions.SocketException, Task>? ExceptionHandler;
        protected Func<SocketSession, Task>? DisconnectHandler;

        public ISocketSessionBind BindByteHandler(Func<SocketSession, Memory<byte>, Task>? byteHandler)
        {
            ByteHandler += byteHandler;
            return this;
        }

        public ISocketSessionBind BindStringHandler(Func<SocketSession, string, Task>? stringHandler)
        {
            StringHandler += stringHandler;
            return this;
        }

        public ISocketSessionBind BindExceptionHandler(Func<SocketSession, Exceptions.SocketException, Task>? exceptionHandler)
        {
            ExceptionHandler += exceptionHandler;
            return this;
        }

        public ISocketSessionBind BindDisconnectHandler(Func<SocketSession, Task>? disconnectHandler)
        {
            DisconnectHandler += disconnectHandler;
            return this;
        }
    }

    public abstract class SocketSessionBase : SocketSessionHandlerBase, ISocketSessionBind, IDisposable
    {
        protected TcpClient tcpClient;

        protected NetworkStream Stream => tcpClient.GetStream();

        public Guid SessionId { get; protected set; }

        public SocketSessionBase(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            Start();
        }

        protected void Start()
        {
            Task<Task>.Factory.StartNew(Receive, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected abstract Task Receive();

        private async ValueTask SendAsync(DataType type, params ReadOnlyMemory<byte>[] data)
        {
            await Stream.WriteAsync(BitConverter.GetBytes(data.Sum(x => x.Length)));
            await Stream.WriteAsync(new byte[] { (byte)type });
            foreach (var item in data)
            {
                await Stream.WriteAsync(item);
            }
        }

        public ValueTask SendStringAsync(string data)
        {
            return SendAsync(DataType.Text, data.GetBytes());
        }

        public ValueTask SendBytesAsync(params ReadOnlyMemory<byte>[] data)
        {
            return SendAsync(DataType.Byte, data);
        }


        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 清理托管资源
                Stream.Dispose();
                tcpClient.Close();
                tcpClient.Dispose();
            }

            // 清理非托管资源
            ByteHandler = null;
            ExceptionHandler = null;
            StringHandler = null;

            _disposed = true;
        }

        #endregion

    }
}
