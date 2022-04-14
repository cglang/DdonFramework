using System;

namespace Ddon.ConvenientSocket.Exceptions
{
    public class DdonSocketDisconnectException : Exception
    {
        public Guid SocketId { get; }

        public DdonSocketDisconnectException(Exception ex, Guid socketId) : base("连接断开", ex)
        {
            SocketId = socketId;
        }
    }
}
