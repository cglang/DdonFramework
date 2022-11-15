using Ddon.Socket.Session.Model;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Socket.Session
{
    public abstract class SocketApiCore
    {
        [AllowNull]
        public virtual SocketSession Session { get; set; }

        [AllowNull]
        public DdonSocketSessionHeadInfo Head { get; set; }
    }
}
