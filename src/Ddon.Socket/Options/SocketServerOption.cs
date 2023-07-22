using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Ddon.Socket.Options
{
    public class SocketServerOption
    {
        [AllowNull]
        public IPEndPoint? IPEndPoint { get; set; }
    }
}
