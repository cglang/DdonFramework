using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ddon.Socket.Core;
using Ddon.Socket.Core.Handle;
using Ddon.TuuTools.Socket.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Socket
{
    [TestClass]
    public class SocketBase
    {
        [TestMethod]
        public async Task TestBaseCon()
        {
            DefaultDataHandler handler = new DefaultDataHandler();

            List<DdonTcpClient> clients = new();
            var _listener = new DdonTcpListener<DefaultDataHandler>(IPAddress.Parse("127.0.0.1"), 9889);
            _listener.Start();

            var t1 = Task.Run(async () => clients.Add(await _listener.AcceptDdonTcpClientAsync()));
            var t2 = Task.Run(() => clients.Add(new DdonTcpClient("127.0.0.1", 9889, handler)));

            await Task.WhenAll(t1, t2);

            Assert.IsTrue(clients.Count == 2);
            Assert.IsTrue(clients.First().Id == clients.Last().Id);
        }

        [TestMethod]
        public async Task TestDataSendingAndReceiving()
        {
            var tcpConnectionPool = new TcpConnectionPool();
            var server = new DdonTcpServer<DefaultDataHandler>(IPAddress.Parse("127.0.0.1"), 8777, tcpConnectionPool);
            server.Start();

            DefaultDataHandler handler = new DefaultDataHandler();
            for (int i = 0; i < 5; i++)
            {
                _ = Task.Run(async () =>
                {
                    var client = new DdonTcpClient("127.0.0.1", 8777, handler);
                    await client.SendStringAsync(i.ToString());
                });
            }
        }
    }

    public class DefaultDataHandler : IDdonTcpClientDataHandler
    {
        public Func<IDdonTcpClient, Memory<byte>, Task>? ByteHandler => async (client, data) =>
        {

        };

        public Func<IDdonTcpClient, string, Task>? StringHandler => async (client, data) =>
        {

        };

        public Func<IDdonTcpClient, DdonSocketException, Task>? ExceptionHandler => async (client, ex) =>
        {
        };

        public Func<IDdonTcpClient, Task>? DisconnectHandler => async (client) =>
        {
        };
    }
}
