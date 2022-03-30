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
    public abstract class DdonSocketConnectionBase : DdonSocketHandler, IDisposable
    {
        /// <summary>
        /// Tcp客户端
        /// </summary>
        protected TcpClient TcpClient { get; set; }

        /// <summary>
        /// SocketId
        /// </summary>
        public Guid SocketId { get; set; }

        /// <summary>
        /// Tcp 流
        /// </summary>
        protected Stream Stream => TcpClient.GetStream();

        #region Handler

        private readonly DdonSocketHandler _handler;

        public override Action<DdonSocketPackageInfo<string>> StringHandler => _handler.StringHandler;

        public override Action<DdonSocketPackageInfo<byte[]>> FileByteHandler => _handler.FileByteHandler;

        public override Action<DdonSocketPackageInfo<Stream>> StreamHandler => _handler.StreamHandler;

        public override Action<Exception> ExceptionHandler => _handler.ExceptionHandler;

        public override Func<DdonSocketPackageInfo<string>, Task<object>> RandQHandler => _handler.RandQHandler;

        #endregion

        protected DdonSocketConnectionBase(string host, int port, DdonSocketHandler handler)
        {
            TcpClient = new TcpClient(host, port);

            _handler = handler;
        }

        protected DdonSocketConnectionBase(TcpClient tcpClient, DdonSocketHandler handler)
        {
            TcpClient = tcpClient;

            _handler = handler;
        }

        /// <summary>
        /// 持续从流中读取数据
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DdonSocketDisconnectException">Socket连接断开异常 携带断开连接Id</exception>
        public async Task ConsecutiveReadStreamAsync()
        {
            // 这里暂时有问题 应当把数据都读取到一个队列里 然后再从队列中取数据来执行其他操作
            // 这类暂时一次只处理一个操作
            while (true)
            {
                try
                {
                    var head = await ReadHeadAsync();

                    if (head.Mode is Mode.Request)
                    {
                        var content = await Stream.ReadStringAsync(head.Length);
                        // 捕获异常
                        var data = await RandQHandler.Invoke(new DdonSocketPackageInfo<string>(this, head, content));
                        Response(head.Id, JsonSerializer.Serialize(data));// await SendStringAsync();
                    }
                    else if (head.Mode is Mode.Response)
                    {
                        var content = await Stream.ReadStringAsync(head.Length);
                        DdonSocketResponseCollection.GetInstance().ResponseHandle(new DdonSocketPackageInfo<string>(this, head, content));
                    }
                    else if (head.Mode is Mode.Normal)
                    {
                        if (head.DataType is DdonSocketDataType.File)
                        {
                            var bytes = await Stream.ReadByteAsync(head.Length);
                            FileByteHandler.Invoke(new DdonSocketPackageInfo<byte[]>(this, head, bytes));
                        }
                        else if (head.DataType is DdonSocketDataType.Byte)
                        {
                            StreamHandler.Invoke(new DdonSocketPackageInfo<Stream>(this, head, Stream));
                        }
                        else if (head.DataType is DdonSocketDataType.String)
                        {
                            var content = await Stream.ReadStringAsync(head.Length);
                            StringHandler.Invoke(new DdonSocketPackageInfo<string>(this, head, content));
                        }
                        else
                        {
                            // 不支持的类型
                        }
                    }
                }
                catch (IOException ex) { throw new DdonSocketDisconnectException(ex, SocketId); }
                catch (Exception ex) { ExceptionHandler.Invoke(ex); }
            }
        }

        /// <summary>
        /// 发送Bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async ValueTask SendBytesAsync(byte[] bytes)
        {
            await Stream.WriteAsync(bytes);
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public async Task<int> SendStringAsync(string data, int opCode = default)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketCommon.GetHeadBytes(opCode, Mode.Normal, DdonSocketDataType.String, dataBytes.Length);
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, dataBytes);

            await SendBytesAsync(contentBytes);
            return dataBytes.Length;
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public DdonSocketResponse Request(int opCode, string data)
        {
            var id = Guid.NewGuid();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketCommon.GetHeadBytes(opCode, Mode.Request, DdonSocketDataType.String, dataBytes.Length, id);
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, dataBytes);
            Task.Run(async () => await SendBytesAsync(contentBytes));
            return new DdonSocketResponse(id);
        }

        public void Response(Guid id, string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketCommon.GetHeadBytes(0, Mode.Response, DdonSocketDataType.String, dataBytes.Length, id);
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, dataBytes);
            Task.Run(async () => await SendBytesAsync(contentBytes));
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
