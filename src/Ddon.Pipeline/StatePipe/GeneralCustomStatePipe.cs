using System;
using System.Threading.Tasks;

namespace Ddon.Pipeline.StatePipe
{

    public class GeneralCustomStatePipe
    {
        public static IGeneralCustomPipeline<TContext> GeneralCustomPipeline<TContext>(params StatePipeMiddleware<TContext>[] middlewares) where TContext : new()
        {
            return GeneralCustomPipelineFactory<TContext>.CreatePipelineBuild().ConfigureMiddlewares(builder =>
            {
                foreach (var middleware in middlewares)
                {
                    builder.AddMiddleware(async context =>
                    {
                        if (middleware != null && await middleware.DecideFunc(context))
                        {
                            await middleware?.LogicBeforeFunc(context);
                            await middleware?.LogicFunc(context);
                            await middleware?.LogicAfterFunc(context);
                        }
                    });
                }
            }).Build();
        }
    }

    public class StatePipeMiddleware<TContext>
    {
        public StatePipeMiddleware(
            Func<TContext, Task<bool>> decideFunc,
            Func<TContext, Task> logicFunc,
            Func<TContext, Task> logicBeforeFunc = null,
            Func<TContext, Task> logicAfterFunc = null)
        {
            DecideFunc = decideFunc;
            LogicFunc = logicFunc;
            LogicBeforeFunc = logicBeforeFunc;
            LogicAfterFunc = logicAfterFunc;
        }

        public Func<TContext, Task<bool>> DecideFunc { get; set; }

        public Func<TContext, Task> LogicFunc { get; set; }

        public Func<TContext, Task> LogicBeforeFunc { get; set; }

        public Func<TContext, Task> LogicAfterFunc { get; set; }
    }
}
