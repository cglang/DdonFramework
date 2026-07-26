using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.VitrinPLC.Clients.Mitsubishi;

public class McProtocol : McProtocolBase
{
    private TcpClient TcpClient { get; set; }
    private NetworkStream TcpClientStream { get; set; } = null!;

    public override bool Connected => TcpClient.Connected;

    public McProtocol(string hostName, int port, McProtocolFrame protocolFrame)
        : base(hostName, port, protocolFrame)
    {
        CommandProtocolFrame = protocolFrame;
        TcpClient = new TcpClient();
    }

    protected override async Task DoConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!TcpClient.Connected)
        {
            TcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            await TcpClient.ConnectAsync(HostName, Port, cancellationToken);
            TcpClientStream = TcpClient.GetStream();
        }
    }

    protected override void DoDisconnect()
    {
        if (TcpClient.Connected)
        {
            TcpClient.Close();
        }
    }

    protected override async Task<byte[]> ExecuteAsync(byte[] iCommand)
    {
        await TcpClientStream.WriteAsync(iCommand);
        TcpClientStream.Flush();

        using var ms = new MemoryStream();
        var buff = new byte[256];
        do
        {
            var sz = TcpClientStream.Read(buff, 0, buff.Length);
            if (sz == 0)
            {
                throw new Exception("TcpClientStream 被切断");
            }

            ms.Write(buff, 0, sz);
        } while (TcpClientStream.DataAvailable);

        return ms.ToArray();
    }
}