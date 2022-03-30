using Ddon.Socket.Extra;

namespace Ddon.Socket.Handler
{
    public abstract class DdonSocketHandler
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

        /// <summary>
        /// 请求相应模式处理器
        /// </summary>
        public abstract Func<DdonSocketPackageInfo<string>, Task<object>> RandQHandler { get; }

        /// <summary>
        /// 发生异常处理器
        /// </summary>
        public abstract Action<Exception> ExceptionHandler { get; }
    }
}
