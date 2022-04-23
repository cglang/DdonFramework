using Ddon.ConvenientSocket.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Socket.Core
{
    public class DdonSocketConnectionCore : IDisposable
    {
        private readonly Func<DdonSocketConnectionCore, byte[], Task> ByteHandler;

        public DdonSocketConnectionCore(TcpClient tcpClient, Func<DdonSocketConnectionCore, byte[], Task> byteHandler)
        {
            TcpClient = tcpClient;
            ByteHandler = byteHandler;
        }

        public Guid SocketId { get; set; } = Guid.NewGuid();

        public Stream Stream => TcpClient.GetStream();

        protected TcpClient TcpClient { get; set; }

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
                    var head = await Stream.ReadLengthBytesAsync(sizeof(int));
                    var length = BitConverter.ToInt32(head);
                    var initialBytes = await Stream.ReadLengthBytesAsync(length);
                    await ByteHandler(this, initialBytes);
                }
                catch (IOException ex) { throw new DdonSocketDisconnectException(ex, SocketId); }
                catch { }
            }
        }

        /// <summary>
        /// 发送Byte数组
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> SendBytesAsync(byte[] data)
        {
            var lengthByte = BitConverter.GetBytes(data.Length);

            byte[] contentBytes = new byte[lengthByte.Length + data.Length];
            Array.Copy(lengthByte, contentBytes, sizeof(int));
            Array.Copy(data, 0, contentBytes, sizeof(int), data.Length);

            await Stream.WriteAsync(contentBytes);
            return data.Length;
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<int> SendStringAsync(string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return await SendBytesAsync(dataBytes);
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<int> SendStringAsync<TData>(TData data)
        {
            return await SendStringAsync(JsonSerializer.Serialize(data));
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="fileFormat">文件格式</param>
        /// <param name="fileStream">文件流对象</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SendFileAsync(string fileFormat, FileStream fileStream)
        {
            var headBytes = Encoding.UTF8.GetBytes(fileFormat);
            if (headBytes.Length > 160) throw new Exception("文件格式字符太长");

            var dataBytes = await fileStream.ReadAllBytesAsync();
            byte[] contentBytes = DdonSocketCommon.MergeArrays(headBytes, 160, dataBytes);

            return await SendBytesAsync(contentBytes);
        }

        public void Dispose()
        {
            if (TcpClient is not null) TcpClient.Close();
            GC.SuppressFinalize(this);
        }
    }
}
