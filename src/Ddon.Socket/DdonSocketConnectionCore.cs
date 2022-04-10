using Ddon.ConvenientSocket.Exceptions;
using Ddon.ConvenientSocket.Extra;
using Ddon.Core;
using Ddon.Socket.Route;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Ddon.Socket
{
    public class DdonSocketConnectionCore : IDisposable
    {
        protected TcpClient TcpClient { get; set; }

        protected Stream Stream => TcpClient.GetStream();

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// SocketId
        /// </summary>
        public Guid SocketId { get; set; } = Guid.NewGuid();

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
        /// 发送Byte数组
        /// </summary>
        /// <param name="route"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> SendBytesAsync(string route, byte[] data)
        {
            var headBytes = DdonSocketHead.GetHeadBytes(Mode.String, data.Length, route);
            byte[] contentBytes = MergeArrays(headBytes, DdonSocketConst.HeadLength, data);

            await Stream.WriteAsync(contentBytes);
            return data.Length;
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<int> SendStringAsync(string route, string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return await SendBytesAsync(route, dataBytes);
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<int> SendStringAsync<TData>(string route, TData data)
        {
            return await SendStringAsync(route, JsonSerializer.Serialize(data));
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="fileFormat">文件格式</param>
        /// <param name="fileStream">文件流对象</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SendFileAsync(string route, string fileFormat, FileStream fileStream)
        {
            var headBytes = Encoding.UTF8.GetBytes(fileFormat);
            if (headBytes.Length > 160) throw new Exception("文件格式字符太长");

            var dataBytes = await fileStream.ReadAllBytesAsync();
            byte[] contentBytes = MergeArrays(headBytes, 160, dataBytes);

            return await SendBytesAsync(route, contentBytes);
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="data"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public DdonSocketResponse Request(string route, string data)
        {
            var id = Guid.NewGuid();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketHead.GetHeadBytes(Mode.Request, dataBytes.Length, route, id);
            byte[] contentBytes = MergeArrays(headBytes, DdonSocketConst.HeadLength, dataBytes);
            Task.Run(async () => await Stream.WriteAsync(contentBytes));
            return new DdonSocketResponse(id);
        }


        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">请求超时异样</exception>
        public async Task<DdonSocketPackageInfo<string>> RequestAsync(string route, string data)
        {
            var taskCompletion = new TaskCompletionSource<DdonSocketPackageInfo<string>>();
            Request(route, data)
                .Then(inf => { taskCompletion.SetResult(inf); })
                .Exception(inf => { taskCompletion.SetException(new DdonSocketRequestException()); });
            return await taskCompletion.Task;
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
                var route = DdonSocketRouteMap.Get(head.Route);
                if (route is null) return;

                if (head.Mode == Mode.String)
                {
                    var data = Encoding.UTF8.GetString(bytes);
                    await DdonSocketInvoke.IvnvokeAsync(_serviceProvider, route.Value.Item1, route.Value.Item2, data, this, head);
                }
                else if (head.Mode == Mode.Byte)
                {
                    await DdonSocketInvoke.IvnvokeAsync(_serviceProvider, route.Value.Item1, route.Value.Item2, bytes, this, head);
                }
                else if (head.Mode == Mode.File)
                {
                    // 前 160 byte 作为文件的格式名称，20个字符。
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", DateTime.Now.ToString("yyyy-MM-dd"));
                    Directory.CreateDirectory(path);
                    var fileFormat = Encoding.UTF8.GetString(ByteCut(bytes[0..160]));
                    var fullName = Path.Combine(path, $"{Guid.NewGuid()}{fileFormat}");

                    using var fileStream = new FileStream(fullName, FileMode.CreateNew);
                    fileStream.Write(bytes, 160, bytes.Length - 160);
                    await DdonSocketInvoke.IvnvokeAsync(_serviceProvider, route.Value.Item1, route.Value.Item2, fileStream, this, head);
                }
                else if (head.Mode == Mode.Request)
                {
                    var data = Encoding.UTF8.GetString(bytes);
                    if (route is not null)
                    {
                        var resData = await DdonSocketInvoke.IvnvokeReturnJsonAsync(_serviceProvider, route.Value.Item1, route.Value.Item2, data, this, head);
                        var dataBytes = Encoding.UTF8.GetBytes(resData);
                        var headBytes = head.Response(dataBytes.Length).GetBytes();
                        byte[] contentBytes = MergeArrays(headBytes, DdonSocketConst.HeadLength, dataBytes);
                        await Stream.WriteAsync(contentBytes);
                    }
                }
            }
        }

        private static byte[] MergeArrays(byte[] array1, int headLength, byte[] array2)
        {
            byte[] contentBytes = new byte[headLength + array2.Length];
            Array.Copy(array1, contentBytes, array1.Length);
            Array.Copy(array2, 0, contentBytes, DdonSocketConst.HeadLength, array2.Length);
            return contentBytes;
        }

        private static byte[] ByteCut(byte[] bytes, byte cut = 0x00)
        {
            List<byte> list = new(bytes);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == cut)
                    list.RemoveAt(i);
            }
            byte[] lastbyte = new byte[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                lastbyte[i] = list[i];
            }
            return lastbyte;
        }

        public void Dispose()
        {
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
