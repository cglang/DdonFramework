﻿using Ddon.Core.Use.Socket.Exceptions;
using Ddon.Socket.Session;
using System;
using System.Threading.Tasks;

namespace Ddon.Socket.Hosting
{
    public class SocketServerOption
    {
        public string Host { get; private set; } = "0.0.0.0";

        public int Port { get; private set; } = 6000;

        ///<summary>
        /// 断开连接是否自动重连
        /// </summary>
        public bool IsReconnection { get; set; }

        public Func<SocketSession, DdonSocketException, Task>? ExceptionHandler { get; private set; }

        public Func<SocketSession, IServiceProvider, Task>? SocketAccessHandler { get; private set; }

        public void SetListenerInfo(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public void SetListenerInfo(int port)
        {
            Port = port;
        }

        public void AddExceptionHandler(Func<SocketSession, DdonSocketException, Task> exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
        }

        public void AddSocketAccessHandler(Func<SocketSession, IServiceProvider, Task> socketAccessHandler)
        {
            SocketAccessHandler = socketAccessHandler;
        }
    }
}
