using Ddon.Common.Utilities.WheelTimer;

namespace Test.Common
{
    [TestClass]
    public sealed class TestWheelTimer
    {
        [TestMethod]
        public void Test()
        {
            var timer = new HashedWheelTimer(
                TimeSpan.FromMilliseconds(10),
                512
            );

            int a = 1;
            timer.NewTimeout(async () =>
            {
                await Task.Delay(100);
                a = 2; 
            }, TimeSpan.FromMilliseconds(100));

            int b = 1;
            timer.NewTimeout(() =>
            {
                b = 2;
            }, TimeSpan.FromMilliseconds(100));

            Assert.AreEqual(1, a);
            Assert.AreEqual(1, b);

            Thread.Sleep(400);

            Assert.AreEqual(2, a);
            Assert.AreEqual(2, b);
        }
    }
}
