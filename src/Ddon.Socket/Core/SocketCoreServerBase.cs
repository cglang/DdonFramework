using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Exceptions;

namespace Ddon.Socket.Core
{
    public abstract class DdonSocketServerHandlerBase : IDdonSocketServerBind
    {
        protected Func<SocketCoreSession, Memory<byte>, Task>? ByteHandler;
        protected Func<SocketCoreSession, string, Task>? StringHandler;
        protected Func<SocketCoreSession, SocketException, Task>? ExceptionHandler;
        protected Func<SocketCoreSession, Task>? DisconnectHandler;
        protected Func<SocketCoreSession, Task>? ConnectHandler;

        public IDdonSocketServerBind BindByteHandler(Func<SocketCoreSession, Memory<byte>, Task>? byteHandler)
        {
            ByteHandler += byteHandler;
            return this;
        }

        public IDdonSocketServerBind BindStringHandler(Func<SocketCoreSession, string, Task>? stringHandler)
        {
            StringHandler += stringHandler;
            return this;
        }

        public IDdonSocketServerBind BindExceptionHandler(Func<SocketCoreSession, SocketException, Task>? exceptionHandler)
        {
            ExceptionHandler += exceptionHandler;
            return this;
        }

        public IDdonSocketServerBind BindDisconnectHandler(Func<SocketCoreSession, Task>? disconnectHandler)
        {
            DisconnectHandler += disconnectHandler;
            return this;
        }

        public IDdonSocketServerBind BindConnectHandler(Func<SocketCoreSession, Task>? connectHandler)
        {
            ConnectHandler += connectHandler;
            return this;
        }
    }

    public abstract class SocketCoreServerBase : DdonSocketServerHandlerBase, IDdonSocketServerBind
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
