using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Core.Enums;
using Ddon.Socket.Core.Handle;
using Ddon.TuuTools.Socket;
using Ddon.TuuTools.Socket.Exceptions;
using Ddon.TuuTools.System;

namespace Ddon.Socket.Core;

public abstract class DdonTcpClientBase : IDdonTcpClient
{
    public Guid Id { get; protected set; }

    public TcpClient TcpClient { get; }

    public NetworkStream Stream => TcpClient.GetStream();

    public bool CanRead => Stream.CanRead && Stream.DataAvailable;

    public DdonTcpClientBase(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
    }

    public void Start() => Task<Task>.Factory.StartNew(StartAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    public async Task StartAsync()
    {
        while (true)
        {
            await Function();
        }
    }

    public abstract Task Function();

    public ValueTask SendBytesAsync(byte[] data, SocketDataType type, CancellationToken cancellationToken)
    {
        var lengthByte = BitConverter.GetBytes(data.Length);
        var typeByte = new[] { (byte)type };

        DdonArray.MergeArrays(out byte[] contentBytes, lengthByte, typeByte, data);
        return Stream.WriteAsync(contentBytes, cancellationToken);
    }

    public ValueTask SendBytesAsync(byte[] data, CancellationToken cancellationToken)
    {
        return SendBytesAsync(data, SocketDataType.Byte, cancellationToken);
    }

    public ValueTask SendBytesAsync(byte[] data)
    {
        return SendBytesAsync(data, SocketDataType.Byte, CancellationToken.None);
    }

    public ValueTask SendStringAsync(string data, CancellationToken cancellationToken)
    {
        return SendBytesAsync(data.GetBytes(), SocketDataType.Text, cancellationToken);
    }

    public ValueTask SendStringAsync(string data)
    {
        return SendStringAsync(data, CancellationToken.None);
    }

    public Task ProcessAsync() => Function();
}

public class DdonTcpClient : DdonTcpClientBase
{
    private readonly IDdonTcpClientDataHandler _dataHandler;

    private DdonTcpClient(TcpClient tcpClient, IDdonTcpClientDataHandler dataHandler) : base(tcpClient)
    {
        _dataHandler = dataHandler;
    }

    internal DdonTcpClient(TcpClient tcpClient, Guid id, IDdonTcpClientDataHandler dataHandler) : this(tcpClient, dataHandler)
    {
        Id = id;
        Stream.Write(Id.ToByteArray());
    }

    public DdonTcpClient(string host, int port, IDdonTcpClientDataHandler dataHandler) : this(new TcpClient(host, port), dataHandler)
    {
        var data = new byte[16];
        var idLength = Stream.Read(data);
        if (idLength != 16) throw new Exception("建立连接失败");

        Id = new Guid(data);
    }

    public override async Task Function()
    {
        try
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
                if (_dataHandler.ExceptionHandler != null)
                    await _dataHandler.ExceptionHandler(this, new(ex, Id));
            }
        }
        catch (Exception ex)
        {
            TcpClient.Dispose();

            if (_dataHandler.ExceptionHandler != null)
            {
                var socketEx = new DdonSocketException(ex, Id);
                await _dataHandler.ExceptionHandler(this, socketEx);
            }

            if (_dataHandler.DisconnectHandler != null)
            {
                await _dataHandler.DisconnectHandler(this);
            }
        }
    }

    private Task InitialHandle(Memory<byte> data, DataType type)
    {
        if (_dataHandler.ByteHandler != null && type == DataType.Byte)
        {
            return _dataHandler.ByteHandler(this, data);
        }
        else if (_dataHandler.StringHandler != null && type == DataType.Text)
        {
            return _dataHandler.StringHandler(this, Encoding.UTF8.GetString(data.Span));
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
            DdonArray.MergeArrays(out var bytes, BitConverter.GetBytes(Length), new[] { (byte)Type });
            return bytes;
        }

        public const int HeadLength = sizeof(int) + sizeof(DataType);
    }
}
