using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Core
{
    [TestClass]
    public class ConnCoreTests
    {
        /// <summary>
        /// ºËĞÄÀà»ù´¡²âÊÔ
        /// </summary>
        [TestMethod]
        public async Task CoreBaseTest()
        {
            var result = string.Empty;
            DdonSocket.CreateServer("127.0.0.1", 5356)
                .StringHandler(async (a, b) =>
                {
                    await Task.CompletedTask;
                    result = b;
                })
                .ByteHandler(async (a, b) =>
                {
                    await Task.CompletedTask;
                    result = Encoding.UTF8.GetString(b);
                })
                .Start();

            var core = DdonSocket.CreateClient("127.0.0.1", 5356);

            string sendData = "test texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest texttest text";
            await core.SendStringAsync(sendData);
            await Task.Delay(100);
            Assert.AreEqual(sendData, result);
        }
    }
}