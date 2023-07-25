using Ddon.Socket.Core;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketPackageInfo<TData>
    {
        public SocketSession Connection { get; set; }

        public DdonSocketSessionHeadInfo Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(SocketSession connection, DdonSocketSessionHeadInfo head, TData data)
        {
            Connection = connection;
            Head = head;
            Data = data;
        }
    }
}
