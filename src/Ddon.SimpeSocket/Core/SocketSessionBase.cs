using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Core;

namespace Ddon.SimpeSocket.Core
{
    public abstract class SocketSessionHandlerBase : ISocketSessionBind
    {
        protected Func<SocketSession, ReadOnlyMemory<byte>, Task> ByteHandler;
        protected Func<SocketSession, string, Task> StringHandler;
        protected Func<SocketSession, Exceptions.SocketException, Task> ExceptionHandler;
        protected Func<SocketSession, Task> DisconnectHandler;

        public ISocketSessionBind BindByteHandler(Func<SocketSession, ReadOnlyMemory<byte>, Task> byteHandler)
        {
            ByteHandler += byteHandler;
            return this;
        }

        public ISocketSessionBind BindStringHandler(Func<SocketSession, string, Task> stringHandler)
        {
            StringHandler += stringHandler;
            return this;
        }

        public ISocketSessionBind BindExceptionHandler(Func<SocketSession, Exceptions.SocketException, Task> exceptionHandler)
        {
            ExceptionHandler += exceptionHandler;
            return this;
        }

        public ISocketSessionBind BindDisconnectHandler(Func<SocketSession, Task> disconnectHandler)
        {
            DisconnectHandler += disconnectHandler;
            return this;
        }
    }

    public abstract class SocketSessionBase : SocketSessionHandlerBase, ISocketSessionBind, IDisposable
    {
        protected TcpClient TcpClient { get; set; }

        protected NetworkStream Stream => TcpClient.GetStream();

        public Guid SessionId { get; protected set; }

        public SocketSessionBase(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            Start();
        }

        protected void Start()
        {
            Task<ValueTask>.Factory.StartNew(Receive, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected abstract ValueTask Receive();

        static ReadOnlyMemory<byte> Combine(DataType type, params ReadOnlyMemory<byte>[] arrays)
        {
            var dataLength = arrays.Sum(item => item.Length);
            var dataLengthArray = BitConverter.GetBytes(dataLength);

            var length = arrays.Sum(item => item.Length) + dataLengthArray.Length + 1;

            var bytes = new byte[length];
            var offset = 0;

            Buffer.BlockCopy(dataLengthArray, 0, bytes, offset, dataLengthArray.Length);
            offset += dataLengthArray.Length;

            Buffer.BlockCopy(new[] { (byte)type }, 0, bytes, offset, 1);
            offset += 1;

            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array.ToArray(), 0, bytes, offset, array.Length);
                offset += array.Length;
            }

            return bytes;
        }
        private async ValueTask SendAsync(DataType type, params ReadOnlyMemory<byte>[] data)
        {
            await Stream.WriteAsync(Combine(type, data));

        }

        public ValueTask SendStringAsync(string data)
        {
            return SendAsync(DataType.Text, Encoding.UTF8.GetBytes(data));
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
                TcpClient.Close();
                TcpClient.Dispose();
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
