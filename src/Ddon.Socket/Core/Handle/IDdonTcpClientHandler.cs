using System;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket.Exceptions;

namespace Ddon.Socket.Core.Handle
{
    public interface IDdonTcpClientDataHandler
    {
        /// <summary>
        /// Byte 数据
        /// </summary>
        Func<IDdonTcpClient, Memory<byte>, Task>? ByteHandler { get; }

        /// <summary>
        /// 文本数据
        /// </summary>
        Func<IDdonTcpClient, string, Task>? StringHandler { get; }

        /// <summary>
        /// 异常处理
        /// </summary>
        Func<IDdonTcpClient, DdonSocketException, Task>? ExceptionHandler { get; }

        /// <summary>
        /// 连接断开
        /// </summary>
        Func<IDdonTcpClient, Task>? DisconnectHandler { get; }
    }

    public interface IDdonTcpServerHandler : IDdonTcpClientDataHandler
    {
        /// <summary>
        /// 连接接入
        /// </summary>
        Func<IDdonTcpClient, Task>? ConnectHandler { get; }
    }
}
