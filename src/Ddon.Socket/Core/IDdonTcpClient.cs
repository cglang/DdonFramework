using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Core.Enums;

namespace Ddon.Socket.Core
{
    public interface IDdonTcpClient
    {
        Guid Id { get; }

        TcpClient TcpClient { get; }

        NetworkStream Stream { get; }

        /// <summary>
        /// 是否有要读取的数据
        /// </summary>
        bool CanRead { get; }

        void Start();

        Task StartAsync();

        Task ProcessAsync();

        ValueTask SendBytesAsync(byte[] data, SocketDataType type, CancellationToken cancellationToken);

        ValueTask SendBytesAsync(byte[] data, CancellationToken cancellationToken);

        ValueTask SendBytesAsync(byte[] data);

        ValueTask SendStringAsync(string data, CancellationToken cancellationToken);

        ValueTask SendStringAsync(string data);
    }
}
