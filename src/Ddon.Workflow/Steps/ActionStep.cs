using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Workflow.Steps
{
    /// <summary>
    /// 逻辑触发步骤（无耗时，仅执行一个委托动作）
    /// </summary>
    public class ActionStep<TContext> : Step<TContext>
    {
        private readonly Func<TContext, Task> _function;
        private bool _done;

        public ActionStep(Func<TContext, Task> function)
        {
            _function = function;
        }

        public override async Task<StepStatus> OnUpdateAsync(TContext context, CancellationToken cancellationToken)
        {
            if (!_done)
            {
                await _function(context);
                _done = true;
            }
            return StepStatus.Success;
        }
    }
}
