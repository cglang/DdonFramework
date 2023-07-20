using System;

namespace Ddon.Core.Use.Socket.Exceptions;

public class DdonSocketException : Exception
{
    public Guid SocketId { get; }

    public DdonSocketException(Exception ex, Guid socketId, string? message = null)
        : base(message ?? ex.Message, ex)
    {
        SocketId = socketId;
    }
}
