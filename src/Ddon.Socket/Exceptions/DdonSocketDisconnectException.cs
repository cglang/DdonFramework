namespace Ddon.Socket.Exceptions
{
    /// <summary>
    /// 连接断开异常
    /// </summary>
    public class DdonSocketDisconnectException : Exception
    {
        public Guid SocketId { get; set; }

        public DdonSocketDisconnectException(Guid socketId) : base("Socket连接断开")
        {
            SocketId = socketId;
        }
    }
}
