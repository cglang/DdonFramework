using Ddon.Socket.Session.Model;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Socket.Session
{
    public class DdonSocketApiCore
    {
        [AllowNull]
        public DdonSocketSession Session { get; set; }

        [AllowNull]
        public DdonSocketRequest Head { get; set; }
    }
}
