using System;
using System.Net.Sockets;
using Ddon.Socket.Configuration;

namespace Ddon.Socket.Abstractions
{
    public interface ISocketFactory
    {
        ISocketWorker CreateWorker(SocketClientOptions options);

        ISocketWorker CreateWorker(TcpClient acceptedClient, SocketClientOptions options);

        ISocketProtocol CreateProtocol(Type protocolType);

        IReconnectStrategy CreateReconnectStrategy(Type strategyType);
    }
}
