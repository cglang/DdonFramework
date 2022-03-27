using Ddon.Socket.Extra;

namespace Ddon.Socket.Handler
{
    public abstract class DdonSocketHandlerBase
    {
        /// <summary>
        /// 文本流处理器
        /// </summary>
        public abstract Action<DdonSocketPackageInfo<string>> StringHandler { get; }

        /// <summary>
        /// 文件流处理器
        /// </summary>
        public abstract Action<DdonSocketPackageInfo<byte[]>> FileByteHandler { get; }

        /// <summary>
        /// Byte 流处理器
        /// </summary>
        public abstract Action<DdonSocketPackageInfo<Stream>> StreamHandler { get; }
    }
}
