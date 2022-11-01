using Ddon.Core.Use.Socket;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketCore Connection { get; set; }

        public DdonSocketRequest Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(DdonSocketCore connection, DdonSocketRequest head,ref TData data)
        {
            Connection = connection;
            Head = head;
            Data = data;
        }
    }
}
