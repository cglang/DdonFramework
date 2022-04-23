using Ddon.Socket.Core;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketConnectionCore Connection { get; set; }

        public DdonSocketRequest Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(DdonSocketConnectionCore connection, DdonSocketRequest head, TData data)
        {
            Connection = connection;
            Head = head;
            Data = data;
        }
    }
}
