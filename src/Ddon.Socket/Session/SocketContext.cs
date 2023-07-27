using System;
using Ddon.Socket.Core;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session
{
    public class SocketContext
    {
        public SocketContext(SocketSession session, SocketHeadInfo head, Memory<byte> dataBytes)
        {
            Session = session;
            Head = head;
            SourceData = dataBytes;
        }

        public SocketSession Session { get; }

        public SocketHeadInfo Head { get; }

        public Memory<byte> SourceData { get; }

        public SocketEndPoint? EndPoint { get; private set; }

        public void SetEndPoint(SocketEndPoint? endPoint)
        {
            EndPoint = endPoint;
        }
    }
}
