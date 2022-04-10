using Ddon.Socket;

namespace Ddon.ConvenientSocket.Extra
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketHead Head { get; }

        public TData Data { get; set; }

        public DdonSocketConnectionCore Connection { get; set; }

        public DdonSocketPackageInfo(DdonSocketConnectionCore connection, DdonSocketHead headDto, TData data)
        {
            Connection = connection;
            Head = headDto;
            Data = data;
        }
    }
}
