using System.Diagnostics.CodeAnalysis;
using Ddon.Socket.Core;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session
{
    public abstract class SocketApiBase
    {
        [AllowNull]
        public SocketSession Session { get; set; }

        [AllowNull]
        public DdonSocketSessionHeadInfo Head { get; set; }
    }
}
