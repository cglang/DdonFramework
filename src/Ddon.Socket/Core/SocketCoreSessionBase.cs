using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Utility;

namespace Ddon.Socket.Core
{
    public abstract class DdonSocketSessionHandlerBase : IDdonSocketSessionBind
    {
        protected Func<SocketCoreSession, Memory<byte>, Task>? ByteHandler;
        protected Func<SocketCoreSession, string, Task>? StringHandler;
        protected Func<SocketCoreSession, Exceptions.SocketException, Task>? ExceptionHandler;
        protected Func<SocketCoreSession, Task>? DisconnectHandler;

        public IDdonSocketSessionBind BindByteHandler(Func<SocketCoreSession, Memory<byte>, Task>? byteHandler)
        {
            ByteHandler += byteHandler;
            return this;
        }

        public IDdonSocketSessionBind BindStringHandler(Func<SocketCoreSession, string, Task>? stringHandler)
        {
            StringHandler += stringHandler;
            return this;
        }

        public IDdonSocketSessionBind BindExceptionHandler(Func<SocketCoreSession, Exceptions.SocketException, Task>? exceptionHandler)
        {
            ExceptionHandler += exceptionHandler;
            return this;
        }

        public IDdonSocketSessionBind BindDisconnectHandler(Func<SocketCoreSession, Task>? disconnectHandler)
        {
            DisconnectHandler += disconnectHandler;
            return this;
        }
    }

    public abstract class SocketCoreSessionBase : DdonSocketSessionHandlerBase, IDdonSocketSessionBind, IDisposable
    {
        protected TcpClient _tcpClient;

        protected NetworkStream Stream => _tcpClient.GetStream();

        public Guid SessionId { get; protected set; }

        public SocketCoreSessionBase(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            Start();
        }

        protected void Start() => Task<Task>.Factory.StartNew(Receive, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        public Task StartAsync() => Receive();

        protected abstract Task Receive();

        /// <summary>
        /// 发送Byte数组
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">发送类型</param>
        /// <returns>发送的数据字节长度</returns>
        public ValueTask SendBytesAsync(byte[] data, DataType type = DataType.Byte)
        {
            var lengthByte = BitConverter.GetBytes(data.Length);
            var typeByte = new[] { (byte)type };

            ByteArrayHelper.MergeArrays(out var contentBytes, lengthByte, typeByte, data);
            return Stream.WriteAsync(contentBytes);
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public ValueTask SendStringAsync(string data)
        {
            return SendBytesAsync(data.GetBytes(), DataType.Text);
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
                _tcpClient.Close();
                _tcpClient.Dispose();
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
