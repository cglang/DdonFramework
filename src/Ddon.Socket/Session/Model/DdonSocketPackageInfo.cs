using Ddon.Core.Use.Socket;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketCore Connection { get; set; }

        public DdonSocketSessionHeadInfo Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(DdonSocketCore connection, DdonSocketSessionHeadInfo head,TData data)
        {
            Connection = connection;
            Head = head;
            Data = data;
        }
    }
}
