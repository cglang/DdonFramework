using Ddon.TuuTools.Socket;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketSession Connection { get; set; }

        public DdonSocketSessionHeadInfo Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(DdonSocketSession connection, DdonSocketSessionHeadInfo head, TData data)
        {
            Connection = connection;
            Head = head;
            Data = data;
        }
    }
}
