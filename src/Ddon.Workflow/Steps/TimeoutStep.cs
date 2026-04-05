using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Workflow.Steps
{
    /// <summary>
    /// 带超时检查的步骤基类
    /// </summary>
    public abstract class TimeoutStep<TContext> : Step<TContext>
    {
        // 开始时间
        private DateTime _start;

        /// <summary>
        /// 默认300秒超时
        /// </summary>
        protected int TimeoutMs = 300 * 1000;

        /// <summary>
        /// 超时时间 默认300秒超时
        /// </summary>
        protected TimeSpan Timeout { get; }

        protected TimeoutStep()
        {
            Timeout = TimeSpan.FromSeconds(300);
        }

        protected TimeoutStep(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public override Task OnEnterAsync(TContext context, CancellationToken cancellationToken)
        {
            _start = DateTime.Now;  // 记录开始时间
            return Task.CompletedTask;
        }

        protected bool IsTimeout()
        {
            return (DateTime.Now - _start) > Timeout;
        }
    }
}
