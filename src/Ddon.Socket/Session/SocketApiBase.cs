using Ddon.Core.Use.Socket;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session
{
    public abstract class SocketApiBase
    {
        public DdonSocketSession? Session { get; set; }

        public DdonSocketSessionHeadInfo? Head { get; set; }
    }
}
