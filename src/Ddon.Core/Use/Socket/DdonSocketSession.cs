using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket.Exceptions;

namespace Ddon.Core.Use.Socket;

public class DdonSocketSession : DdonSocketSessionBase, IDisposable
{
    private readonly TcpClient _tcpClient;

    public DdonSocketSession(TcpClient tcpClient) : base(tcpClient.GetStream())
    {
        _tcpClient = tcpClient;

        var data = new byte[16];
        tcpClient.GetStream().Read(data);
        SocketId = new Guid(data);
    }

    public DdonSocketSession(TcpClient tcpClient, Guid socketId) : base(tcpClient.GetStream())
    {
        _tcpClient = tcpClient;
        
        SocketId = socketId;
        tcpClient.GetStream().Write(SocketId.ToByteArray());       
    }

    public DdonSocketSession(string host, int port) : this(new TcpClient(host, port)) { }

    public DdonSocketSession(
        TcpClient tcpClient,
        Func<DdonSocketSession, Memory<byte>, Task>? byteHandler,
        Func<DdonSocketSession, DdonSocketException, Task>? exceptionHandler = null) : this(tcpClient)
    {
        ByteHandler += byteHandler;
        ExceptionHandler += exceptionHandler;
    }

    protected override async Task Function()
    {
        try
        {
            while (true)
            {
                var headBytes = await Stream.ReadLengthAsync(Head.HeadLength);
                var head = new Head(headBytes);

                if (head.Length == 0) throw new Exception("Socket 连接已断开");

                var initial = await Stream.ReadLengthAsync(head.Length);
                try
                {
                    await InitialHandle(initial, head.Type);
                }
                catch (Exception ex)
                {
                    if (ExceptionHandler != null)
                        await ExceptionHandler(this, new(ex, SocketId));
                }
            }
        }
        catch (Exception ex)
        {
            _tcpClient.Dispose();

            if (ExceptionHandler != null)
            {
                var socketEx = new DdonSocketException(ex, SocketId);
                await ExceptionHandler(this, socketEx);
            }

            if (DisconnectHandler != null)
            {
                await DisconnectHandler(this);
            }
        }
    }

    private Task InitialHandle(Memory<byte> data, DataType type)
    {
        if (ByteHandler != null && type == DataType.Byte)
        {
            return ByteHandler(this, data);
        }
        else if (StringHandler != null && type == DataType.Text)
        {
            return StringHandler(this, Encoding.UTF8.GetString(data.Span));
        }
        else
        {            
            return Task.CompletedTask;
        }
    }

    #region Dispose

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
            Stream.Dispose();
            _tcpClient.Close();
            _tcpClient.Dispose();
        }

        // 清理非托管资源
        ByteHandler = null;
        ExceptionHandler = null;
        StringHandler = null;

        _disposed = true;
    }

    #endregion

    private readonly struct Head
    {
        public int Length { get; }
        public DataType Type { get; }

        public Head(Memory<byte> bytes)
        {
            Length = BitConverter.ToInt32(bytes.Span[..sizeof(int)]);
            Type = (DataType)bytes.Span[sizeof(int)];
        }

        public byte[] GetBytes()
        {
            ByteArrayHelper.MergeArrays(out var bytes, BitConverter.GetBytes(Length), new[] { (byte)Type });
            return bytes;
        }

        public const int HeadLength = sizeof(int) + sizeof(DataType);
    }
}

public enum DataType : byte
{
    Text = 1,
    Byte = 2
}
