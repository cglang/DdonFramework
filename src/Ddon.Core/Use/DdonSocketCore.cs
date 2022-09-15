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
        private readonly Func<DdonSocketCore, byte[], Task> ByteHandler;
        private readonly Func<DdonSocketCore, DdonSocketException, Task>? ExceptionHandler;


        public DdonSocketCore(
            TcpClient tcpClient,
            Func<DdonSocketCore, byte[], Task> byteHandler,
            Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler = null)
        {
            TcpClient = tcpClient;
            ByteHandler = byteHandler;
            ExceptionHandler = exceptionHandler;

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
                        await ByteHandler(this, initialBytes);
                    }
                }
                catch (Exception ex)
                {
                    if (ExceptionHandler != null)
                    {
                        var socketEx = new DdonSocketException(ex, SocketId);
                        await ExceptionHandler(this, socketEx);
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
        /// 发送字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendStringAsync<TData>(TData data)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            return await SendStringAsync(JsonSerializer.Serialize(data, options));
        }

        public void Dispose()
        {
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
