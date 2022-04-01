using Ddon.ConvenientSocket;
using Ddon.ConvenientSocket.Exceptions;
using Ddon.ConvenientSocket.Extra;
using Ddon.Core;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Ddon.Socket.Connection
{
    public class DdonSocketConnectionCore : IDisposable
    {
        protected TcpClient TcpClient { get; set; }

        protected Stream Stream => TcpClient.GetStream();

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// SocketId
        /// </summary>
        public Guid SocketId { get; set; }

        public DdonSocketConnectionCore(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            _serviceProvider = ServiceProviderFactory.GetServiceProvider();
        }

        /// <summary>
        /// 持续从流中读取数据
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DdonSocketDisconnectException">Socket连接断开异常 携带断开连接Id</exception>
        public async Task ConsecutiveReadStreamAsync()
        {
            while (true)
            {
                try
                {
                    var head = await ReadHeadAsync();
                    var bytes = await Stream.ReadLengthBytesAsync(head.Length);
                    await ByteHandler(head, bytes);
                }
                catch (IOException ex) { throw new DdonSocketDisconnectException(ex, SocketId); }
                catch { }
            }
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public async Task<int> SendStringAsync(string data, string? route = default)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketHead.GetHeadBytes(Mode.String, dataBytes.Length, route);
            byte[] contentBytes = MergeArrays(headBytes, dataBytes);

            await Stream.WriteAsync(contentBytes);
            return dataBytes.Length;
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="data"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public DdonSocketResponse Request(string data, string? route = default)
        {
            var id = Guid.NewGuid();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketHead.GetHeadBytes(Mode.Request, dataBytes.Length, route, id);
            byte[] contentBytes = MergeArrays(headBytes, dataBytes);
            Task.Run(async () => await Stream.WriteAsync(contentBytes));
            return new DdonSocketResponse(id);
        }

        /// <summary>
        /// 从流中获取消息头
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">获取消息头错误</exception>
        private async Task<DdonSocketHead> ReadHeadAsync()
        {
            var bytes = await Stream.ReadLengthBytesAsync(DdonSocketConst.HeadLength);

            var headDto = JsonSerializer.Deserialize<DdonSocketHead>(Encoding.UTF8.GetString(bytes))
                ?? throw new Exception("消息中不包含消息头");

            return headDto;
        }

        /// <summary>
        /// 数据处理器
        /// </summary>
        private async Task ByteHandler(DdonSocketHead head, byte[] bytes)
        {
            if (head.Mode == Mode.Response)
            {
                var content = Encoding.UTF8.GetString(bytes);
                DdonSocketResponseCollection.GetInstance().ResponseHandle(new DdonSocketPackageInfo<string>(this, head, content));
            }
            else
            {
                var tm = DdonSocketRouteMap.Get(head.Route);
                if (head.Mode == Mode.String)
                {
                    var data = Encoding.UTF8.GetString(bytes);
                    if (tm is not null)
                        await DdonInvoke.IvnvokeAsync(_serviceProvider, tm.Value.Item1, tm.Value.Item2, data);
                }
                else if (head.Mode == Mode.Byte)
                {
                    // 处理Byte -> 依旧保持byte数组
                }
                else if (head.Mode == Mode.File)
                {
                    // 处理文件 -> 会直接转为一个文件对象
                }
                else if (head.Mode == Mode.Request)
                {
                    var data = Encoding.UTF8.GetString(bytes);
                    if (tm is not null)
                    {
                        var resData = await DdonInvoke.IvnvokeReturnJsonAsync(_serviceProvider, tm.Value.Item1, tm.Value.Item2, data);
                        var dataBytes = Encoding.UTF8.GetBytes(resData);
                        var headBytes = head.Response(dataBytes.Length).GetBytes();
                        byte[] contentBytes = MergeArrays(headBytes, dataBytes);
                        await Stream.WriteAsync(contentBytes);
                    }
                }
            }
        }

        private static byte[] MergeArrays(byte[] array1, byte[] array2)
        {
            byte[] contentBytes = new byte[DdonSocketConst.HeadLength + array2.Length];
            Array.Copy(array1, contentBytes, array1.Length);
            Array.Copy(array2, 0, contentBytes, DdonSocketConst.HeadLength, array2.Length);
            return contentBytes;
        }

        public void Dispose()
        {
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
