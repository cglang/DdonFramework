using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Ddon.Socket.Options
{
    public class SocketServerOption
    {
        [AllowNull, NotNull]
        public IPEndPoint IPEndPoint { get; set; }
    }
}
