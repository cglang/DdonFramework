using System;

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
                    builder.AddMiddleware(context =>
                    {
                        if (middleware?.DecideFunc(context) ?? false)
                        {
                            middleware?.LogicBeforeFunc(context);
                            middleware?.LogicFunc(context);
                            middleware?.LogicAfterFunc(context);
                        }
                    });
                }
            }).Build();
        }
    }

    public class StatePipeMiddleware<TContext>
    {
        public StatePipeMiddleware(
            Func<TContext, bool> decideFunc,
            Action<TContext> logicFunc,
            Action<TContext> logicBeforeFunc = null,
            Action<TContext> logicAfterFunc = null)
        {
            DecideFunc = decideFunc;
            LogicFunc = logicFunc;
            LogicBeforeFunc = logicBeforeFunc;
            LogicAfterFunc = logicAfterFunc;
        }

        public Func<TContext, bool> DecideFunc { get; set; }

        public Action<TContext> LogicFunc { get; set; }

        public Action<TContext> LogicBeforeFunc { get; set; }

        public Action<TContext> LogicAfterFunc { get; set; }
    }
}
