using Ddon.Socket.Extensions;
using Ddon.Socket.Extra;
using Ddon.Socket.Handler;

namespace Ddon.Socket.Connection
{
    // TODO 应该再加个有service版的
    public class DdonSocketConnectionClient<TDdonSocketHandler> : DdonSocketConnectionBase<TDdonSocketHandler>
        where TDdonSocketHandler : DdonSocketHandlerBase, new()
    {

        public DdonSocketConnectionClient(string host, int port) : base(host, port)
        {
            SocketId = new Guid(Stream.ReadByte(16));
        }

        public async Task<int> SendAuthenticationInfoAsync(byte[] dataBytes)
        {
            var headBytes = DdonSocketCommon.GetHeadBytes(DdonSocketOpCode.Authentication, DdonSocketDataType.String, dataBytes.Length, SocketId, default);
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, dataBytes);
            await Stream.WriteAsync(contentBytes);
            return dataBytes.Length;
        }
    }
}
