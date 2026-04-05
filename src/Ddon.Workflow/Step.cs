using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;

namespace Ddon.Workflow
{
    /// <summary>
    /// 抽象步骤基类：所有的动作或逻辑都继承此类
    /// </summary>
    public abstract class Step<TContext> : IStep<TContext>
    {
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 进入该步骤时的初始化（仅执行一次）
        /// </summary>
        public virtual Task OnEnterAsync(TContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 步骤执行中的逻辑（轮询执行）
        /// </summary>
        public abstract Task<StepStatus> OnUpdateAsync(TContext context, CancellationToken cancellationToken);

        /// <summary>
        /// 步骤完成后的清理（仅执行一次）
        /// </summary>
        public virtual Task OnExitAsync(TContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
