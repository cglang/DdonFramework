using Ddon.Socket.Handler;

namespace Ddon.Socket.Extra
{
    public class DdonSocketPackageInfo<TData>
    {
        public DdonSocketHeadDto Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(DdonSocketHeadDto headDto, TData data)
        {
            Head = headDto;
            Data = data;
        }
    }
}
