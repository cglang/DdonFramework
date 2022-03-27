using Ddon.Socket.Exceptions;
using Ddon.Socket.Extensions;
using Ddon.Socket.Extra;
using Ddon.Socket.Handler;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Ddon.Socket.Connection
{
    // TODO 应该再加个有service版的
    public abstract class DdonSocketConnectionBase<TDdonSocketHandler> : DdonSocketHandler<TDdonSocketHandler>, IDisposable
        where TDdonSocketHandler : DdonSocketHandlerBase, new()
    {
        /// <summary>
        /// Tcp客户端
        /// </summary>
        protected TcpClient TcpClient { get; set; }

        /// <summary>
        /// 客户端Id
        /// </summary>
        public Guid SocketId { get; set; }

        /// <summary>
        /// Tcp 流
        /// </summary>
        protected Stream Stream => TcpClient.GetStream();

        protected DdonSocketConnectionBase(string host, int port)
        {
            TcpClient = new TcpClient(host, port);
        }

        protected DdonSocketConnectionBase(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }

        /// <summary>
        /// 持续从流中读取数据
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DdonSocketDisconnectException">Socket连接断开异常 携带断开连接Id</exception>
        public async Task ConsecutiveReadStreamAsync()
        {
            try
            {
                await Task.Run(InitialStreamHandler);
            }
            catch
            {
                throw new DdonSocketDisconnectException(SocketId);
            }
        }

        private async Task InitialStreamHandler()
        {
            while (true)
            {
                var head = await ReadHeadAsync();

                if (head.Mode is Mode.Normal) { }
                else if (head.Mode is Mode.RandQ) { }
                else { } // 不支持的模式

                if (head.DataType is DdonSocketDataType.File)
                {
                    var bytes = await Stream.ReadByteAsync(head.Length);
                    FileByteHandler?.Invoke(new DdonSocketPackageInfo<byte[]>(head, bytes));
                }
                else if (head.DataType is DdonSocketDataType.Byte)
                {
                    base.StreamHandler?.Invoke(new DdonSocketPackageInfo<Stream>(head, Stream));
                }
                else if (head.DataType is DdonSocketDataType.String)
                {
                    var content = await Stream.ReadStringAsync(head.Length);
                    StringHandler?.Invoke(new DdonSocketPackageInfo<string>(head, content));
                }
                else
                {
                    // 不支持的类型
                }
            }
        }

        public async ValueTask SendBytesAsync(byte[] bytes)
        {
            await Stream.WriteAsync(bytes);
        }

        public async Task<int> SendStringAsync(string data, int opCode = default,
            Guid sendClientId = default, Guid sendGroupId = default, Guid clientId = default, Guid groupId = default)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketCommon.GetHeadBytes(opCode, DdonSocketDataType.String, dataBytes.Length, clientId, groupId, sendClientId, sendGroupId);
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, dataBytes);

            await Stream.WriteAsync(contentBytes);
            return dataBytes.Length;
        }

        /// <summary>
        /// 从流中获取消息头
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">获取消息头错误</exception>
        private async Task<DdonSocketHeadDto> ReadHeadAsync()
        {
            var bytes = await Stream.ReadByteAsync(DdonSocketConst.HeadLength);

            var headDto = JsonSerializer.Deserialize<DdonSocketHeadDto>(Encoding.UTF8.GetString(bytes)) ??
                throw new Exception("消息中不包含消息头");

            return headDto;
        }

        public void Dispose()
        {
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
