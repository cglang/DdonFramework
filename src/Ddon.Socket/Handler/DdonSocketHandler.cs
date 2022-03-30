using Ddon.Socket.Extra;

namespace Ddon.Socket.Handler
{
    public class DdonSocketHandler<TDdonSocketHandler> : DdonSocketHandlerBase where TDdonSocketHandler : DdonSocketHandlerBase, new()
    {
        private readonly Action<DdonSocketPackageInfo<string>> _stringHandler;
        public override Action<DdonSocketPackageInfo<string>> StringHandler => _stringHandler;

        private readonly Action<DdonSocketPackageInfo<byte[]>> _fileByteHandler;
        public override Action<DdonSocketPackageInfo<byte[]>> FileByteHandler => _fileByteHandler;

        private readonly Action<DdonSocketPackageInfo<Stream>> _streamHandler;
        public override Action<DdonSocketPackageInfo<Stream>> StreamHandler => _streamHandler;

        private readonly Action<Exception> _connDisconnecHandler;
        public override Action<Exception> ExceptionHandler => _connDisconnecHandler;

        public DdonSocketHandler()
        {
            var handler = new TDdonSocketHandler();
            _stringHandler = handler.StringHandler;
            _fileByteHandler = handler.FileByteHandler;
            _streamHandler = handler.StreamHandler;
            _connDisconnecHandler = handler.ExceptionHandler;
        }
    }
}
