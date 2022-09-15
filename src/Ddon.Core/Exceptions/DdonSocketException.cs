using System;

namespace Ddon.Core.Exceptions
{
    public class DdonSocketException : Exception
    {
        public Guid SocketId { get; }

        public DdonSocketException(Exception ex, Guid socketId, string? message = null) : base(message, ex)
        {
            SocketId = socketId;
        }
    }
}
