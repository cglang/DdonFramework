using Ddon.Core.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Core.Use
{
    public class DdonSocketCore : IDisposable
    {
        private Func<DdonSocketCore, byte[], Task>? byteHandler;
        private Func<DdonSocketCore, string, Task>? stringHandler;
        private Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler;


        public DdonSocketCore(
            TcpClient tcpClient,
            Func<DdonSocketCore, byte[], Task> byteHandler,
            Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler = null)
        {
            TcpClient = tcpClient;
            this.byteHandler = byteHandler;
            this.exceptionHandler = exceptionHandler;

            ConsecutiveReadStream();
        }

        public DdonSocketCore(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            ConsecutiveReadStream();
        }

        public Guid SocketId { get; } = Guid.NewGuid();

        public Stream Stream => TcpClient.GetStream();

        protected TcpClient TcpClient { get; set; }

        private void ConsecutiveReadStream()
        {
            var newThread = new Thread(async () =>
            {
                try
                {
                    while (true)
                    {
                        var dataSize = await Stream.ReadLengthBytesAsync(sizeof(int));
                        var length = BitConverter.ToInt32(dataSize);
                        var initialBytes = await Stream.ReadLengthBytesAsync(length);
                        if (byteHandler != null)
                        {
                            await byteHandler(this, initialBytes);
                        }
                        if (stringHandler != null)
                        {
                            var data = Encoding.UTF8.GetString(initialBytes);
                            await stringHandler(this, data ?? string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                    {
                        var socketEx = new DdonSocketException(ex, SocketId);
                        await exceptionHandler(this, socketEx);
                    }
                }
            });
            newThread.Start();
        }

        /// <summary>
        /// 发送Byte数组
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendBytesAsync(byte[] data)
        {
            var lengthByte = BitConverter.GetBytes(data.Length);
            byte[] contentBytes = DdonArray.MergeArrays(lengthByte, data);

            await Stream.WriteAsync(contentBytes);
            return data.Length;
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendStringAsync(string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return await SendBytesAsync(dataBytes);
        }

        /// <summary>
        /// 发送Json字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendJsonAsync<TData>(TData data)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            return await SendStringAsync(JsonSerializer.Serialize(data, options));
        }

        public DdonSocketCore ByteHandler(Func<DdonSocketCore, byte[], Task> byteHandler)
        {
            this.byteHandler = byteHandler;
            return this;
        }

        public DdonSocketCore StringHandler(Func<DdonSocketCore, string, Task> stringHandler)
        {
            this.stringHandler = stringHandler;
            return this;
        }

        public DdonSocketCore ExceptionHandler(Func<DdonSocketCore, DdonSocketException, Task> exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler;
            return this;
        }

        public void Dispose()
        {
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
