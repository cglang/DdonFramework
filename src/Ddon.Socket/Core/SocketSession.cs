﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ddon.Socket.Core;

public partial class SocketSession : SocketSessionBase
{
    public SocketSession(TcpClient tcpClient) : base(tcpClient)
    {
        SessionId = new Guid(Stream.ReadLength(16).Span);
    }

    public SocketSession(TcpClient tcpClient, Guid socketId) : base(tcpClient)
    {
        SessionId = socketId;
        tcpClient.GetStream().Write(SessionId.ToByteArray());
    }

    public SocketSession(string host, int port) : this(new TcpClient(host, port)) { }

    protected void InitConnect(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        SessionId = new Guid(Stream.ReadLength(16).Span);
    }

    public void Reconnect(TcpClient tcpClient)
    {
        TcpClient.Dispose();
        InitConnect(tcpClient);
        Start();
    }

    protected override async ValueTask Receive()
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
                    if (head.Type is DataType.Text && StringHandler is not null)
                        _ = StringHandler(this, Encoding.UTF8.GetString(initial.Span));
                    if (head.Type is DataType.Byte && ByteHandler is not null)
                        _ = ByteHandler(this, initial);
                }
                catch (Exception ex)
                {
                    if (ExceptionHandler is not null)
                        _ = ExceptionHandler(this, new(ex, SessionId));
                }
            }
        }
        catch (Exception ex)
        {
            if (ExceptionHandler is not null)
                _ = ExceptionHandler(this, new(ex, SessionId));

            if (DisconnectHandler is not null)
                _ = DisconnectHandler(this);
        }
    }
}
