using System;

namespace Ddon.Socket.Exceptions;

public class SocketException : Exception
{
    public Guid SocketId { get; }

    public SocketException(Exception ex, Guid socketId, string? message = null)
        : base(message ?? ex.Message, ex)
    {
        SocketId = socketId;
    }
}
