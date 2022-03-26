using System.Diagnostics.CodeAnalysis;

namespace DdonSocket.Extra
{
    public class DdonSocketPackageInfo<TData>
    {
        public IServiceProvider? ServiceProvider { get; set; }

        public DdonSocketHeadDto Head { get; }

        public TData Data { get; set; }

        public DdonSocketPackageInfo(IServiceProvider? serviceProvider,
            DdonSocketHeadDto headDto,
            TData data)
        {
            ServiceProvider = serviceProvider;
            Head = headDto;
            Data = data;
        }
    }
}
