using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Socket;

public class SocketCoreSession : SocketCoreSessionBase
{    
    public SocketCoreSession(TcpClient tcpClient) : base(tcpClient)
    {
        var data = new byte[16];
        tcpClient.GetStream().Read(data);
        SessionId = new Guid(data);
    }

    public SocketCoreSession(TcpClient tcpClient, Guid socketId) : base(tcpClient)
    {
        SessionId = socketId;
        tcpClient.GetStream().Write(SessionId.ToByteArray());
    }

    public SocketCoreSession(string host, int port) : this(new TcpClient(host, port)) { }

    public SocketCoreSession(
        TcpClient tcpClient,
        Func<SocketCoreSession, Memory<byte>, Task>? byteHandler,
        Func<SocketCoreSession, Exceptions.SocketException, Task>? exceptionHandler = null) : this(tcpClient)
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
                        await ExceptionHandler(this, new(ex, SessionId));
                }
            }
        }
        catch (Exception ex)
        {
            _tcpClient.Dispose();

            if (ExceptionHandler != null)
            {
                var socketEx = new Exceptions.SocketException(ex, SessionId);
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
