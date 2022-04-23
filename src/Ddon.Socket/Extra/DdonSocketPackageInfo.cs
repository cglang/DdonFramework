using Ddon.Socket;

namespace Ddon.ConvenientSocket.Extra
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketHeadOld Head { get; }

        public TData Data { get; set; }

        public DdonSocketConnection Connection { get; set; }

        public DdonSocketPackageInfo(DdonSocketConnection connection, DdonSocketHeadOld headDto, TData data)
        {
            Connection = connection;
            Head = headDto;
            Data = data;
        }
    }
}
