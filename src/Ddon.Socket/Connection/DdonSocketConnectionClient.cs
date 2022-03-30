using Ddon.Socket.Extensions;
using Ddon.Socket.Extra;
using Ddon.Socket.Handler;

namespace Ddon.Socket.Connection
{
    // TODO 应该再加个有service版的
    public class DdonSocketConnectionClient<TDdonSocketHandler> : DdonSocketConnectionBase
        where TDdonSocketHandler : DdonSocketHandler, new()
    {

        public DdonSocketConnectionClient(string host, int port) : base(host, port, new TDdonSocketHandler())
        {
            SocketId = new Guid(Stream.ReadByte(16));
        }

        public async Task<int> SendAuthenticationInfoAsync(byte[] dataBytes)
        {
            var headBytes = DdonSocketCommon.GetHeadBytes(DdonSocketOpCode.Authentication, Mode.Normal, DdonSocketDataType.String, dataBytes.Length);
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, dataBytes);
            await Stream.WriteAsync(contentBytes);
            return dataBytes.Length;
        }
    }
}
