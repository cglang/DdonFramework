using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions.Persistence;

namespace Ddon.Workflow.Abstractions
{
    public interface IWorkflowScheduler
    {
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="workflow">工作流</param>
        Task StartAsync(IWorkflow workflow, CancellationToken cancellationToken = default);

        /// <summary>
        /// 帧更新
        /// </summary>
        Task UpdateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 从持久化存储中恢复工作流
        /// </summary>
        Task RecoverPersistedWorkflowsAsync(Func<IWorkflowCheckpoint, Task<IWorkflow>> recoveryFactory, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取活跃工作流
        /// </summary>
        IReadOnlyList<IWorkflow> GetActiveWorkflows();
    }
}
