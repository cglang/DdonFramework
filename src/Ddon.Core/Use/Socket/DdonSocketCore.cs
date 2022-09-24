using Ddon.Core.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Socket
{
    public class DdonSocketCore : IDisposable
    {
        public const byte Close = 0xC0; // 关闭
        public const byte Text = 0x0A; // 文本

        public const byte Byte = 0x0B; // 字节流
        // public const byte File = 0x0C; // 文件


        private readonly TcpClient _tcpClient;
        private readonly Stream _stream;

        private Func<DdonSocketCore, byte[], Task>? _byteHandler;
        private Func<DdonSocketCore, string, Task>? _stringHandler;
        private Func<DdonSocketCore, DdonSocketException, Task>? _exceptionHandler;

        public Guid SocketId { get; }

        public DdonSocketCore(TcpClient tcpClient)
        {
            SocketId = Guid.NewGuid();
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();
            ConsecutiveReadStream();
        }

        public DdonSocketCore(string host, int port) : this(new TcpClient(host, port))
        {
        }

        public DdonSocketCore(
            TcpClient tcpClient,
            Func<DdonSocketCore, byte[], Task>? byteHandler,
            Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler = null) : this(tcpClient)
        {
            _byteHandler += byteHandler;
            _exceptionHandler += exceptionHandler;
        }

        private void ConsecutiveReadStream()
        {
            var newThread = new Thread(Start);
            newThread.Start();
        }

        private async void Start()
        {
            try
            {
                while (true)
                {
                    var typeBytes = await _stream.ReadLengthBytesAsync(sizeof(byte));
                    if (typeBytes[0] == Close) throw new Exception("客户端关闭连接");

                    var lengthBytes = await _stream.ReadLengthBytesAsync(sizeof(int));
                    var length = BitConverter.ToInt32(lengthBytes);
                    var initialBytes = await _stream.ReadLengthBytesAsync(length);

                    if (_byteHandler != null && typeBytes[0] == Byte)
                    {
                        await _byteHandler(this, initialBytes);
                    }
                    else if (_stringHandler != null && typeBytes[0] == Text)
                    {
                        var data = Encoding.UTF8.GetString(initialBytes);
                        await _stringHandler(this, data);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_exceptionHandler != null)
                {
                    var socketEx = new DdonSocketException(ex, SocketId);
                    await _exceptionHandler(this, socketEx);
                }
            }
        }


        /// <summary>
        /// 发送Byte数组
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">发送类型</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendBytesAsync(byte[] data, byte type = Byte)
        {
            var typeByte = new[] { type };
            var lengthByte = BitConverter.GetBytes(data.Length);
            var contentBytes = DdonArray.MergeArrays(typeByte, lengthByte, data);

            await _stream.WriteAsync(contentBytes);
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
            return await SendBytesAsync(dataBytes, Text);
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

        public DdonSocketCore ByteHandler(Func<DdonSocketCore, byte[], Task>? byteHandler)
        {
            this._byteHandler += byteHandler;
            return this;
        }

        public DdonSocketCore StringHandler(Func<DdonSocketCore, string, Task>? stringHandler)
        {
            _stringHandler += stringHandler;
            return this;
        }

        public DdonSocketCore ExceptionHandler(Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler)
        {
            _exceptionHandler += exceptionHandler;
            return this;
        }

        public void Dispose()
        {
            _stream.Write(BitConverter.GetBytes(Close));
            _tcpClient.Close();
            _tcpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}