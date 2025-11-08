using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.SimpeSocket.Exceptions;
using Ddon.Socket.Core;

namespace Ddon.SimpeSocket.Core
{
    public abstract class DdonSocketServerHandlerBase : ISocketServerBind
    {
        protected Func<SocketSession, ReadOnlyMemory<byte>, Task> ByteHandler;
        protected Func<SocketSession, string, Task> StringHandler;
        protected Func<SocketSession, SocketException, Task> ExceptionHandler;
        protected Func<SocketSession, Task> DisconnectHandler;
        protected Func<SocketSession, Task> ConnectHandler;

        public ISocketServerBind BindByteHandler(Func<SocketSession, ReadOnlyMemory<byte>, Task> byteHandler)
        {
            ByteHandler += byteHandler;
            return this;
        }

        public ISocketServerBind BindStringHandler(Func<SocketSession, string, Task> stringHandler)
        {
            StringHandler += stringHandler;
            return this;
        }

        public ISocketServerBind BindExceptionHandler(Func<SocketSession, SocketException, Task> exceptionHandler)
        {
            ExceptionHandler += exceptionHandler;
            return this;
        }

        public ISocketServerBind BindDisconnectHandler(Func<SocketSession, Task> disconnectHandler)
        {
            DisconnectHandler += disconnectHandler;
            return this;
        }

        public ISocketServerBind BindConnectHandler(Func<SocketSession, Task> connectHandler)
        {
            ConnectHandler += connectHandler;
            return this;
        }
    }

    public abstract class SocketServerCoreBase : DdonSocketServerHandlerBase, ISocketServerBind
    {
        /// <summary>
        /// 在新线程中启动Socket服务
        /// </summary>
        public void Start() => Task.Run(() => StartAccept(CancellationToken.None));

        /// <summary>
        /// 异步方式启动Socket服务
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken = default) => StartAccept(cancellationToken);

        /// <summary>
        /// 开始接受连接接入
        /// </summary>
        protected abstract Task StartAccept(CancellationToken cancellationToken = default);
    }
}
