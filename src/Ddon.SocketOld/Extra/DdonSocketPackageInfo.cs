using Ddon.Socket.Connection;
using Ddon.Socket.Handler;

namespace Ddon.Socket.Extra
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketHeadDto Head { get; }

        public TData Data { get; set; }

        public DdonSocketConnectionBase Connection { get; set; }

        public DdonSocketPackageInfo(DdonSocketConnectionBase connection, DdonSocketHeadDto headDto, TData data)
        {
            Connection = connection;
            Head = headDto;
            Data = data;
        }
    }
}
