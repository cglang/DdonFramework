using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket.Exceptions;

namespace Ddon.Core.Use.Socket
{
    public abstract class DdonSocketServerHandlerBase : IDdonSocketServerBindHandler
    {
        protected Func<DdonSocketSession, Memory<byte>, Task>? ByteHandler;
        protected Func<DdonSocketSession, string, Task>? StringHandler;
        protected Func<DdonSocketSession, DdonSocketException, Task>? ExceptionHandler;
        protected Func<DdonSocketSession, Task>? DisconnectHandler;
        protected Func<DdonSocketSession, Task>? ConnectHandler;

        public IDdonSocketServerBindHandler BindByteHandler(Func<DdonSocketSession, Memory<byte>, Task>? byteHandler)
        {
            ByteHandler += byteHandler;
            return this;
        }

        public IDdonSocketServerBindHandler BindStringHandler(Func<DdonSocketSession, string, Task>? stringHandler)
        {
            StringHandler += stringHandler;
            return this;
        }

        public IDdonSocketServerBindHandler BindExceptionHandler(Func<DdonSocketSession, DdonSocketException, Task>? exceptionHandler)
        {
            ExceptionHandler += exceptionHandler;
            return this;
        }

        public IDdonSocketServerBindHandler BindDisconnectHandler(Func<DdonSocketSession, Task>? disconnectHandler)
        {
            DisconnectHandler += disconnectHandler;
            return this;
        }

        public IDdonSocketServerBindHandler BindConnectHandler(Func<DdonSocketSession, Task>? connectHandler)
        {
            ConnectHandler += connectHandler;
            return this;
        }
    }

    public abstract class DdonSocketServerBase : DdonSocketServerHandlerBase, IDdonSocketServerBindHandler
    {
        /// <summary>
        /// 在新线程中启动Socket服务
        /// </summary>
        public void Start() => Task<Task>.Factory.StartNew(Function, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        /// <summary>
        /// 异步方式启动Socket服务
        /// </summary>
        public Task StartAsync() => Function();

        protected abstract Task Function();
    }
}
