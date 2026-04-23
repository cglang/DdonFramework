using System.Threading.Tasks;
using System.Threading;

namespace Ddon.Workflow.Abstractions
{
    public interface IStep
    {
        /// <summary>
        /// 步骤 Id
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// 步骤名称
        /// </summary>
        string Name { get; set; }
    }

    public interface IStep<TContext> : IStep
    {
        /// <summary>
        /// 进入该步骤时的初始化（仅执行一次）
        /// </summary>
        Task OnEnterAsync(TContext context, CancellationToken cancellationToken);

        /// <summary>
        /// 步骤执行中的逻辑（轮询执行）
        /// </summary>
        Task<StepStatus> OnUpdateAsync(TContext context, CancellationToken cancellationToken);

        /// <summary>
        /// 步骤完成后的清理（仅执行一次）
        /// </summary>
        Task OnExitAsync(TContext context, CancellationToken cancellationToken);

        /// <summary>
        /// Step 拓展点，允许在 Step 的生命周期内插入自定义逻辑
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        Step<TContext> AddExtension(IStepExtension<TContext> extension);
    }
}
