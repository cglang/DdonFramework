using Ddon.Core.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Socket
{
    public class DdonSocketCore : IDisposable
    {
        private static class Config
        {
            public const byte Text = 0x0A; // 文本
            public const byte Byte = 0x0B; // 字节流

            public const int DATA_LENGTH_SIZE = sizeof(int);

            public const int TYPE_LENGTH_SIZE = sizeof(byte);

            public const int HEAD_LENGTH = DATA_LENGTH_SIZE + TYPE_LENGTH_SIZE;
        }


        private readonly TcpClient _tcpClient;
        private readonly Stream _stream;

        private Func<DdonSocketCore, Memory<byte>, Task>? _byteHandler;
        private Func<DdonSocketCore, string, Task>? _stringHandler;
        private Func<DdonSocketCore, DdonSocketException, Task>? _exceptionHandler;

        public Guid SocketId { get; } = Guid.NewGuid();

        public DdonSocketCore(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();
            ConsecutiveReadStream();
        }

        public DdonSocketCore(string host, int port) : this(new TcpClient(host, port))
        {
        }

        public DdonSocketCore(
            TcpClient tcpClient,
            Func<DdonSocketCore, Memory<byte>, Task>? byteHandler,
            Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler = null) : this(tcpClient)
        {
            _byteHandler += byteHandler;
            _exceptionHandler += exceptionHandler;
        }

        private void ConsecutiveReadStream()
        {
            Task.Run(Start);
        }

        private async Task Start()
        {
            try
            {
                while (true)
                {
                    var head = await _stream.ReadLengthAsync(Config.HEAD_LENGTH);

                    var dataLength = BitConverter.ToInt32(head.Span[..Config.DATA_LENGTH_SIZE]);
                    var type = head.Span[Config.DATA_LENGTH_SIZE];

                    if (dataLength == 0) throw new Exception("Socket 连接已断开");

                    var initial = await _stream.ReadLengthAsync(dataLength);
                    await InitialHandle(initial, type);
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

        private async Task InitialHandle(Memory<byte> data, byte type)
        {
            if (_byteHandler != null && type == Config.Byte)
            {
                await _byteHandler(this, data);
            }
            else if (_stringHandler != null && type == Config.Text)
            {
                await _stringHandler(this, Encoding.UTF8.GetString(data.Span));
            }
        }


        /// <summary>
        /// 发送Byte数组
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">发送类型</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendBytesAsync(byte[] data, byte type = Config.Byte)
        {
            var typeByte = new[] { type };
            var lengthByte = BitConverter.GetBytes(data.Length);

            DdonArray.MergeArrays(out byte[] contentBytes, lengthByte, typeByte, data);
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
            return await SendBytesAsync(data.GetBytes(), Config.Text);
        }

        /// <summary>
        /// 发送Json字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendJsonAsync<TData>(TData data)
        {
            return await SendStringAsync(JsonSerialize(data));
        }

        public DdonSocketCore ByteHandler(Func<DdonSocketCore, Memory<byte>, Task>? byteHandler)
        {
            _byteHandler += byteHandler;
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

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 清理托管资源
                _stream.Dispose();
                _tcpClient.Close();
                _tcpClient.Dispose();
            }

            // 清理非托管资源
            _byteHandler = null;
            _exceptionHandler = null;
            _stringHandler = null;

            _disposed = true;
        }

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public static string JsonSerialize<T>(T data)
        {
            return JsonSerializer.Serialize(data, options);
        }

        public static T? JsonDeserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, options);
        }

        public static T? JsonDeserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data, options);
        }
    }
}