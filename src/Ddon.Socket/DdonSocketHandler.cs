using DdonSocket.Extra;

namespace DdonSocket
{
    public class DdonSocketHandler<TDdonSocketHandler> : DdonSocketHandlerCore where TDdonSocketHandler : DdonSocketHandlerCore, new()
    {
        private readonly Action<DdonSocketPackageInfo<string>> _stringHandler;
        public override Action<DdonSocketPackageInfo<string>> StringHandler => _stringHandler;

        private readonly Action<DdonSocketPackageInfo<byte[]>> _fileByteHandler;
        public override Action<DdonSocketPackageInfo<byte[]>> FileByteHandler => _fileByteHandler;

        private readonly Action<DdonSocketPackageInfo<Stream>> _streamHandler;
        public override Action<DdonSocketPackageInfo<Stream>> StreamHandler => _streamHandler;

        public DdonSocketHandler()
        {
            var handler = new TDdonSocketHandler();
            _stringHandler = handler.StringHandler;
            _fileByteHandler = handler.FileByteHandler;
            _streamHandler = handler.StreamHandler;
        }
    }
}
