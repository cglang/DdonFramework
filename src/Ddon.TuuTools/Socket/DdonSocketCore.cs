using System.Net.Sockets;
using System.Text;
using Ddon.TuuTools.Socket.Exceptions;
using Ddon.TuuTools.Socket.Handler;
using Ddon.TuuTools.System;

namespace Ddon.TuuTools.Socket;

public class DdonSocketCore : DdonSocketCoreHandler, IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly Stream _stream;

    public Guid SocketId { get; } = Guid.NewGuid();

    public DdonSocketCore(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _stream = _tcpClient.GetStream();
        ConsecutiveReadStream();
    }

    public DdonSocketCore(string host, int port) : this(new TcpClient(host, port)) { }

    public DdonSocketCore(
        TcpClient tcpClient,
        Func<DdonSocketCore, Memory<byte>, Task>? byteHandler,
        Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler = null) : this(tcpClient)
    {
        ByteHandler += byteHandler;
        ExceptionHandler += exceptionHandler;
    }

    private void ConsecutiveReadStream()
    {
        Task<Task>.Factory.StartNew(Start, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task Start()
    {
        try
        {
            while (true)
            {
                var headBytes = await _stream.ReadLengthAsync(Head.HeadLength);
                var head = new Head(headBytes);

                if (head.Length == 0) throw new Exception("Socket 连接已断开");

                var initial = await _stream.ReadLengthAsync(head.Length);
                await InitialHandle(initial, head.Type);
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
    }

    private Task InitialHandle(Memory<byte> data, Type type)
    {
        try
        {
            if (ByteHandler != null && type == Type.Byte)
            {
                return ByteHandler(this, data);
            }
            else if (StringHandler != null && type == Type.Text)
            {
                return StringHandler(this, Encoding.UTF8.GetString(data.Span));
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            if (ExceptionHandler != null)
                return ExceptionHandler(this, new(ex, this.SocketId));
            return Task.CompletedTask;
        }
    }


    /// <summary>
    /// 发送Byte数组
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="type">发送类型</param>
    /// <returns>发送的数据字节长度</returns>
    public async Task<int> SendBytesAsync(byte[] data, Type type = Type.Byte)
    {
        var lengthByte = BitConverter.GetBytes(data.Length);
        var typeByte = new[] { (byte)type };

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
        return await SendBytesAsync(data.GetBytes(), Type.Text);
    }

    public DdonSocketCore BindByteHandler(Func<DdonSocketCore, Memory<byte>, Task>? byteHandler)
    {
        ByteHandler += byteHandler;
        return this;
    }

    public DdonSocketCore BindStringHandler(Func<DdonSocketCore, string, Task>? stringHandler)
    {
        StringHandler += stringHandler;
        return this;
    }

    public DdonSocketCore BindExceptionHandler(Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler)
    {
        ExceptionHandler += exceptionHandler;
        return this;
    }

    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
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
        ByteHandler = null;
        ExceptionHandler = null;
        StringHandler = null;

        _disposed = true;
    }

    private readonly struct Head
    {
        public int Length { get; }
        public Type Type { get; }

        public Head(Memory<byte> bytes)
        {
            Length = BitConverter.ToInt32(bytes.Span[..sizeof(int)]);
            Type = (Type)bytes.Span[sizeof(int)];
        }

        public byte[] GetBytes()
        {
            DdonArray.MergeArrays(out var bytes, BitConverter.GetBytes(Length), new[] { (byte)Type });
            return bytes;
        }

        public const int HeadLength = sizeof(int) + sizeof(Type);
    }

    public enum Type : byte
    {
        Text = 0,
        Byte = 1
    }
}
