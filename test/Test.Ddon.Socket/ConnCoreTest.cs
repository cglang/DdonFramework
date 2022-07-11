using Ddon.Core.Use;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Test.Ddon.Socket
{
    [TestClass]
    public class ConnCoreTest
    {
        /// <summary>
        /// ºËÐÄÀà»ù´¡²âÊÔ
        /// </summary>
        [TestMethod]
        public async Task CoreBaseTest()
        {
            var result = string.Empty;

            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5356);
            listener.Start();
            _ = Task.Run(() =>
            {
                var client = listener.AcceptTcpClient();
                SocketCore core = new(client, async (a, b) =>
                {
                    await Task.CompletedTask;
                    result = Encoding.UTF8.GetString(b);
                });
            });

            var tcpClient = new TcpClient("127.0.0.1", 5356);
            SocketCore core = new(tcpClient, async (a, b) =>
            {
                await Task.CompletedTask;
            });
            string sendData = "test text";
            await core.SendStringAsync(sendData);
            await Task.Delay(100);
            Assert.AreEqual(sendData, result);
        }
    }
}