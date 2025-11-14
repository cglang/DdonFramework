using Ddon.Common.Utilities;
using Ddon.Common.Utilities.WheelTimer;

namespace Test.Common
{
    [TestClass]
    public sealed class TestDelayQueue
    {
        [TestMethod]
        public async Task Test()
        {
            var queue = new DelayQueue<int>();
            queue.Enqueue(1, TimeSpan.FromMilliseconds(400));
            queue.Enqueue(2, TimeSpan.FromMilliseconds(200));

            var data = await queue.DequeueAsync();
            Assert.AreEqual(2, data);

            data = await queue.DequeueAsync();
            Assert.AreEqual(1, data);
        }
    }
}
