using Ddon.Pipeline.StatePipe;

namespace Test.Pipeline
{
    [TestClass]
    public sealed class TestPipeline
    {
        [TestMethod]
        public async Task Test()
        {
            int a = 0, b = 0, c = 0, d = 0, e = 0, f = 0;

            await GeneralCustomStatePipe.GeneralCustomPipeline(
                new StatePipeMiddleware<object>(
                    con => Task.FromResult(false),
                    async con => { a = 1; await Task.Delay(100); },
                    async con => { b = 1; await Task.Delay(100); },
                    async con => { c = 1; await Task.Delay(100); }
                ),
                new StatePipeMiddleware<object>(
                    con => Task.FromResult(true),
                    async con => { d = 1; await Task.Delay(100); },
                    async con => { e = 1; await Task.Delay(100); },
                    async con => { f = 1; await Task.Delay(100); }
                )
            ).ExecuteAsync(new object());

            Assert.AreEqual(0, a);
            Assert.AreEqual(0, b);
            Assert.AreEqual(0, c);
            Assert.AreEqual(1, d);
            Assert.AreEqual(1, e);
            Assert.AreEqual(1, f);
        }
    }
}
