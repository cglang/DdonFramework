using System;
using Ddon.Socket.Core;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session.Pipeline
{
    public class SocketContext
    {
        public SocketContext(SocketSession session, SocketSessionHeadInfo head, Memory<byte> dataBytes)
        {
            Session = session;
            Head = head;
            SourceData = dataBytes;
        }

        public SocketSession Session { get; set; }

        public SocketSessionHeadInfo Head { get; set; }

        public Memory<byte> SourceData { get; }
    }
}
