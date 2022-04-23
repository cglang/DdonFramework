using Ddon.ConvenientSocket.Extra;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Socket
{
    public class DdonSocketControllerBase
    {
        [AllowNull]
        public DdonSocketConnection Connection { get; set; }

        [AllowNull]
        public DdonSocketHeadOld Head { get; set; }
    }
}
