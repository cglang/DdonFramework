using Ddon.Socket.Core;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session.Pipeline
{
    public class SocketContext
    {
        public SocketContext(SocketSession session, SocketSessionHeadInfo head)
        {
            Session = session;
            Head = head;
        }

        public SocketSession Session { get; set; }

        public SocketSessionHeadInfo Head { get; set; }
    }
}
