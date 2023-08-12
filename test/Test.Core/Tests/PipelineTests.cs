using System.Threading.Tasks;
using Ddon.Core.Use.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Core.Tests
{
    [TestClass]
    public class PipelineTests
    {
        [TestMethod]
        public async Task MiddlewareBaseTest()
        {
            var pipeline = GeneralCustomPipelineFactory<DataContext>.CreatePipelineBuild().ConfigureMiddlewares(s =>
            {
                s.AddMiddleware<SampleOneMiddleware>();
                s.AddMiddleware<SampleTwoMiddleware>();
            }).Build();

            var dataContext = new DataContext();
            await pipeline.ExecuteAsync(dataContext);

            Assert.AreEqual(dataContext.Context, "abBA");
        }
    }


    public class DataContext
    {
        public string Context { get; set; } = string.Empty;
    }

    public class SampleOneMiddleware : IGeneralPipeline<DataContext>
    {
        public async Task InvokeAsync(DataContext context, PipelineDelegate<DataContext> next)
        {
            context.Context += "a";
            await next(context);
            context.Context += "A";
        }
    }

    public class SampleTwoMiddleware : IGeneralPipeline<DataContext>
    {
        public async Task InvokeAsync(DataContext context, PipelineDelegate<DataContext> next)
        {
            context.Context += "b";
            await next(context);
            context.Context += "B";
        }
    }
}
