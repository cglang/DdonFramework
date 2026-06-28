using System;
using System.Text;
using Ddon.Socket.Core;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session
{
    public class SocketContext
    {
        public SocketContext(SocketSession session, string text)
        {
            Session = session;
            Head = new SocketHeadInfo(default, SocketMode.String, string.Empty);
            SourceData = Encoding.UTF8.GetBytes(text);
        }

        public SocketContext(SocketSession session, SocketHeadInfo head, ReadOnlyMemory<byte> dataBytes)
        {
            Session = session;
            Head = head;
            SourceData = dataBytes;
        }

        public SocketSession Session { get; }

        public SocketHeadInfo Head { get; }

        public ReadOnlyMemory<byte> SourceData { get; }

        public SocketEndPoint? EndPoint { get; private set; }

        public void SetEndPoint(SocketEndPoint? endPoint)
        {
            EndPoint = endPoint;
        }
    }
}
