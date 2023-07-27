using Ddon.Socket.Core;

namespace Ddon.Socket.Session.Model
{
    public class SocketPackageInfo<TData>
    {
        public SocketSession Connection { get; set; }

        public SocketSessionHeadInfo Head { get; }

        public TData Data { get; set; }

        public SocketPackageInfo(SocketSession connection, SocketSessionHeadInfo head, TData data)
        {
            Connection = connection;
            Head = head;
            Data = data;
        }
    }
}
