using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ddon.TuuTools.Socket.Exceptions;
using Ddon.TuuTools.System;

namespace Ddon.TuuTools.Socket.Handler
{
    public abstract class DdonSocketSessionHandlerBase : IDdonSocketSessionBindHandler
    {
        protected Func<DdonSocketSession, Memory<byte>, Task>? ByteHandler;
        protected Func<DdonSocketSession, string, Task>? StringHandler;
        protected Func<DdonSocketSession, DdonSocketException, Task>? ExceptionHandler;
        protected Func<DdonSocketSession, Task>? DisconnectHandler;

        public IDdonSocketSessionBindHandler BindByteHandler(Func<DdonSocketSession, Memory<byte>, Task>? byteHandler)
        {
            ByteHandler += byteHandler;
            return this;
        }

        public IDdonSocketSessionBindHandler BindStringHandler(Func<DdonSocketSession, string, Task>? stringHandler)
        {
            StringHandler += stringHandler;
            return this;
        }

        public IDdonSocketSessionBindHandler BindExceptionHandler(Func<DdonSocketSession, DdonSocketException, Task>? exceptionHandler)
        {
            ExceptionHandler += exceptionHandler;
            return this;
        }

        public IDdonSocketSessionBindHandler BindDisconnectHandler(Func<DdonSocketSession, Task>? disconnectHandler)
        {
            DisconnectHandler += disconnectHandler;
            return this;
        }
    }

    public abstract class DdonSocketSessionBase : DdonSocketSessionHandlerBase, IDdonSocketSession, IDdonSocketSessionBindHandler
    {
        protected readonly NetworkStream Stream;

        public Guid SocketId { get; protected set; }

        public DdonSocketSessionBase(NetworkStream stream)
        {
            Stream = stream;
        }

        public void Start() => Task<Task>.Factory.StartNew(Function, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        public Task StartAsync() => Function();

        protected abstract Task Function();

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

            DdonArray.MergeArrays(out byte[] contentBytes, lengthByte, typeByte, data);
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
    }

    public abstract class DdonSocketServerBase : DdonSocketSessionHandlerBase, IDdonSocketServerBindHandler
    {
        protected Func<DdonSocketSession, Task>? ConnectHandler;

        public IDdonSocketServerBindHandler BindConnectHandler(Func<DdonSocketSession, Task>? connectHandler)
        {
            ConnectHandler += connectHandler;
            return this;
        }

        public void Start() => Task<Task>.Factory.StartNew(Function, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        public Task StartAsync() => Function();

        protected abstract Task Function();
    }

    public interface IDdonSocketSession
    {
        Guid SocketId { get; }

        void Start();

        Task StartAsync();

        ValueTask SendBytesAsync(byte[] data, DataType type = DataType.Byte);

        ValueTask SendStringAsync(string data);
    }
}
