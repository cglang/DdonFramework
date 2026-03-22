using System;

namespace Ddon.ICWorkflow.Steps
{
    /// <summary>
    /// 逻辑触发步骤（无耗时，仅执行一个委托动作）
    /// </summary>
    public class ActionStep<TContext> : Step<TContext>
    {
        private readonly Action _action;
        private bool _done;

        public ActionStep(Action act)
        {
            _action = act;
        }

        public override StepStatus OnUpdate(TContext context)
        {
            if (!_done)
            {
                _action();
                _done = true;
            }
            return StepStatus.Success;
        }
    }
}
