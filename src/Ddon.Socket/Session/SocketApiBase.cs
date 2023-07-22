using Ddon.Socket.Core;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session
{
    public abstract class SocketApiBase
    {
        public SocketCoreSession? Session { get; set; }

        public DdonSocketSessionHeadInfo? Head { get; set; }
    }
}
