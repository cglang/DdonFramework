using System;
using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    public class DecisionPipelineMiddleware<TContext>
    {
        public DecisionPipelineMiddleware(
            Func<TContext, Task<bool>> decideFunc,
            Func<TContext, Task> logicBeforeFunc,
            Func<TContext, Task> logicFunc,
            Func<TContext, Task> logicAfterFunc)
        {
            DecideFunc = decideFunc;
            LogicFunc = logicFunc;
            LogicBeforeFunc = logicBeforeFunc;
            LogicAfterFunc = logicAfterFunc;
        }

        public DecisionPipelineMiddleware(
            Func<TContext, Task<bool>> decideFunc,
            Func<TContext, Task> logicFunc)
        {
            DecideFunc = decideFunc;
            LogicFunc = logicFunc;
        }

        public Func<TContext, Task<bool>> DecideFunc { get; set; }

        public Func<TContext, Task> LogicFunc { get; set; }

        public Func<TContext, Task> LogicBeforeFunc { get; set; }

        public Func<TContext, Task> LogicAfterFunc { get; set; }
    }
}
