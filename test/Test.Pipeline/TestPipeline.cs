using Ddon.Pipeline;

namespace Test.Pipeline
{
    [TestClass]
    public sealed class TestPipeline
    {
        [TestMethod]
        public async Task TestGeneralCustomPipeline()
        {
            int a = 0, b = 0, c = 0;
            var builder = GeneralCustomPipelineFactory<object>.CreatePipelineBuild().ConfigureMiddlewares(builder =>
            {
                builder.AddMiddleware(con =>
                {
                    a = 1;
                    return Task.CompletedTask;
                });
                builder.AddMiddleware(async con =>
                {
                    await Task.Delay(0);
                    b = 1;
                });
            });
            var pipeline1 = builder.Build();
            await pipeline1.ExecuteAsync(new object());

            Assert.AreEqual(1, a);
            Assert.AreEqual(1, b);
            Assert.AreEqual(0, c);

            builder.ConfigureMiddlewares(builder =>
            {
                builder.AddMiddleware(async con =>
                {
                    await Task.Delay(0);
                    b = 2;
                    c = 2;
                });
            });

            var pipeline2 = builder.Build();
            await pipeline2.ExecuteAsync(new object());

            Assert.AreEqual(1, a);
            Assert.AreEqual(2, b);
            Assert.AreEqual(2, c);

            await pipeline1.ExecuteAsync(new object());
            Assert.AreEqual(1, a);
            Assert.AreEqual(1, b);
            Assert.AreEqual(2, c);
        }

        [TestMethod]
        public async Task TestGeneralCustomStatePipe()
        {
            int a = 0, b = 0, c = 0, d = 0, e = 0, f = 0;

            await DecisionPipeline.Build(
                new DecisionPipelineMiddleware<object>(
                    con => Task.FromResult(false),
                    async con => { a = 1; await Task.Delay(100); },
                    async con => { b = 1; await Task.Delay(100); },
                    async con => { c = 1; await Task.Delay(100); }
                ),
                new DecisionPipelineMiddleware<object>(
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

        [TestMethod]
        public async Task TestGeneralCustomStatePipe2()
        {
            int a = 0, d = 0;

            await DecisionPipeline.Build(
                new DecisionPipelineMiddleware<object>(
                    con => Task.FromResult(false),
                    async con => { a = 1; await Task.Delay(100); }
                ),
                new DecisionPipelineMiddleware<object>(
                    con => Task.FromResult(true),
                    async con => { d = 1; await Task.Delay(100); }
                )
            ).ExecuteAsync(new object());

            Assert.AreEqual(0, a);
            Assert.AreEqual(1, d);
        }
    }
}
