using System;

namespace Ddon.ICWorkflow.Steps
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

        public override void OnEnter(TContext context)
        {
            _start = DateTime.Now; // 记录开始时间
        }

        protected bool IsTimeout()
        {
            return (DateTime.Now - _start).TotalMilliseconds > TimeoutMs;
        }
    }
}
