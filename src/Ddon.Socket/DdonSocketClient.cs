using Ddon.Socket.Extra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Ddon.Socket
{
    public class DdonSocketClient<TDdonSocketHandler> : DdonSocketHandler<TDdonSocketHandler>, IDisposable where TDdonSocketHandler : DdonSocketHandlerCore, new()
    {
        protected const int GuidLength = 16;

        private readonly IServiceProvider? _serviceProvider;

        private readonly ILogger<DdonSocketClient<TDdonSocketHandler>> _logger;

        /// <summary>
        /// 客户端Id
        /// </summary>
        public Guid ClientId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tcp客户端
        /// </summary>
        protected TcpClient TcpClient { get; set; }

        /// <summary>
        /// Tcp 流
        /// </summary>
        protected Stream Stream => TcpClient.GetStream();

        public DdonSocketClient(string host, int port,
            ILogger<DdonSocketClient<TDdonSocketHandler>> logger)
        {
            TcpClient = new TcpClient(host, port);
            var dataTask = ReadStreamAsync(GuidLength);
            dataTask.Wait();
            ClientId = new Guid(dataTask.Result);

            _logger = logger;
        }

        protected DdonSocketClient(TcpClient tcpClient, IServiceProvider serviceProvider)
        {
            TcpClient = tcpClient;
            Stream.Write(ClientId.ToByteArray(), 0, GuidLength);

            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<DdonSocketClient<TDdonSocketHandler>>>();
        }

        public async Task<int> SendStringAsync(string data, Guid sendClientId = default)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var headBytes = DdonSocketCommon.GetHeadBytes(200, DdonSocketDataType.String, dataBytes.Length, ClientId, sendClientId, default);
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, dataBytes);

            await Stream.WriteAsync(contentBytes);
            return dataBytes.Length;
        }

        protected async Task<byte[]> ReadStreamAsync(int readLength)
        {
            byte[] bytes = new byte[readLength];
            await Stream.ReadAsync(bytes.AsMemory(0, readLength));
            return DdonSocketCommon.ByteCut(bytes);
        }

        public DdonSocketClient<TDdonSocketHandler> StartRead()
        {
            Task.Run(async () =>
            {
                await ConsecutiveReadStreamAsync();
                _logger.LogInformation($"与服务端断开连接");
            });

            return this;
        }

        protected async Task<Guid> ConsecutiveReadStreamAsync()
        {
            try
            {
                while (true)
                {
                    var head = await ReadHeadAsync();
                    if (head.Type is DdonSocketDataType.File)
                    {
                        var bytes = await ReadByteContentAsync(head.Length);
                        FileByteHandler?.Invoke(new DdonSocketPackageInfo<byte[]>(_serviceProvider, head, bytes));
                    }
                    else if (head.Type is DdonSocketDataType.Byte)
                    {
                        StreamHandler?.Invoke(new DdonSocketPackageInfo<Stream>(_serviceProvider, head, Stream));
                    }
                    else if (head.Type is DdonSocketDataType.String)
                    {
                        var content = await ReadStringContentAsync(head.Length);
                        StringHandler?.Invoke(new DdonSocketPackageInfo<string>(_serviceProvider, head, content.ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{}", e.Message);
                return ClientId;
            }
        }

        private async Task<DdonSocketHeadDto> ReadHeadAsync()
        {
            var bytes = await ReadStreamAsync(DdonSocketConst.HeadLength);
            bytes = DdonSocketCommon.ByteCut(bytes);
            return JsonSerializer.Deserialize<DdonSocketHeadDto>(Encoding.UTF8.GetString(bytes)) ?? new();
        }

        private async Task<StringBuilder> ReadStringContentAsync(long readLength)
        {
            byte[] bytes;
            StringBuilder content = new();
            if (readLength == 0) return content;
            while (readLength != 0)
            {
                bytes = await ReadStreamAsync(DdonSocketConst.Count);
                content.Append(Encoding.UTF8.GetString(bytes));
                readLength -= bytes.Length;
            }
            return content;
        }

        private async Task<byte[]> ReadByteContentAsync(long readLength)
        {
            byte[] bytes = Array.Empty<byte>();
            if (readLength == 0) return bytes;
            while (readLength != 0)
            {
                bytes = await ReadStreamAsync(DdonSocketConst.Count);
                readLength -= bytes.Length;
            }
            return bytes;
        }

        public void Dispose()
        {
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
