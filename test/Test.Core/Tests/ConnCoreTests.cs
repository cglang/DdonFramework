using Ddon.TuuTools.Socket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace Test.Core.Tests
{
    [TestClass]
    public class ConnCoreTests
    {
        /// <summary>
        /// 核心类基础测试
        /// </summary>
        [TestMethod]
        public async Task CoreBaseTest()
        {
            var result = string.Empty;
            DdonSocket.CreateServer("127.0.0.1", 5356)
                .BindStringHandler(async (a, b) =>
                {
                    await Task.CompletedTask;
                    result = b;
                })
                .BindByteHandler(async (a, b) =>
                {
                    await Task.CompletedTask;
                    result = Encoding.UTF8.GetString(b.Span);
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
