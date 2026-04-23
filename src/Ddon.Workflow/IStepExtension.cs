using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;

namespace Ddon.Workflow
{
    /// <summary>
    /// 步骤扩展点：在步骤 OnEnterAsync / OnExitAsync 执行后会被调用
    /// </summary>
    public interface IStepExtension<TContext>
    {
        /// <summary>
        /// 在步骤 OnEnterAsync 完成后执行
        /// </summary>
        Task AfterEnterAsync(IStep<TContext> step, TContext context, CancellationToken cancellationToken);

        /// <summary>
        /// 在步骤 OnExitAsync 完成后执行
        /// </summary>
        Task AfterExitAsync(IStep<TContext> step, TContext context, CancellationToken cancellationToken);
    }
}
