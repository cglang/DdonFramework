using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Ddon.Socket.Options
{
    public class SocketServerOption : SocketOptionBase
    {
        [AllowNull, NotNull]
        public IPEndPoint IPEndPoint { get; set; }
    }
}
