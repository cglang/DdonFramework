using Ddon.Core.Use;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketPackageInfo<TData>
    {
        public SocketCore Connection { get; set; }

        public DdonSocketRequest Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(SocketCore connection, DdonSocketRequest head, TData data)
        {
            Connection = connection;
            Head = head;
            Data = data;
        }
    }
}
