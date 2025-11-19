using System.Linq;

namespace Ddon.Pipeline
{
    public class DecisionPipeline
    {
        public static IGeneralCustomPipeline<TContext> Build<TContext>(params DecisionPipelineMiddleware<TContext>[] middlewares) where TContext : new()
        {
            return GeneralCustomPipelineFactory<TContext>.CreatePipelineBuild().ConfigureMiddlewares(builder =>
            {
                foreach (var (middleware, index) in middlewares.Select((value, index) => (value, index)))
                {
                    builder.AddMiddleware(async context =>
                    {
                        if (middleware != null && await middleware.DecideFunc(context))
                        {
                            if (middleware.LogicBeforeFunc != null)
                                await middleware.LogicBeforeFunc(context);

                            if (middleware.LogicFunc != null)
                                await middleware.LogicFunc(context);

                            if (middleware.LogicAfterFunc != null)
                                await middleware.LogicAfterFunc(context);
                        }
                    });
                }
            }).Build();
        }
    }
}
