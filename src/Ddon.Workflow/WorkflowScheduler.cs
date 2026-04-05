using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Abstractions.Persistence;
using Ddon.Workflow.Persistence;
using Microsoft.Extensions.Logging;

namespace Ddon.Workflow
{
    /// <summary>
    /// 工作流调度器，支持持久化和恢复
    /// </summary>
    public class WorkflowScheduler : IWorkflowScheduler
    {
        private readonly IList<IWorkflow> _workflows;
        private readonly ILogger<IWorkflowScheduler> _logger;
        private readonly IWorkflowRecoveryService _recoveryService;
        private readonly IWorkflowPersistenceStrategy _persistenceStrategy;

        public WorkflowScheduler(
            ILogger<IWorkflowScheduler> logger,
            IWorkflowRecoveryService recoveryService = null,
            IWorkflowPersistenceStrategy persistenceStrategy = null)
        {
            _workflows = new List<IWorkflow>();
            _logger = logger;
            _recoveryService = recoveryService;
            _persistenceStrategy = persistenceStrategy;
        }

        /// <summary>
        /// 恢复存储中的持久化工作流
        /// </summary>
        public async Task RecoverPersistedWorkflowsAsync(
            Func<IWorkflowCheckpoint, Task<IWorkflow>> recoveryFactory,
            CancellationToken cancellationToken = default)
        {
            if (_recoveryService == null)
            {
                _logger.LogWarning("[调度器] 恢复服务不可用");
                return;
            }

            try
            {
                var checkpoints = await _recoveryService.GetRecoverableCheckpointsAsync(cancellationToken);

                if (checkpoints.Length == 0)
                {
                    _logger.LogInformation("[调度器] 没有持久化的工作流需要恢复");
                    return;
                }

                _logger.LogInformation($"[调度器] 尝试恢复 {checkpoints.Length} 个工作流");

                foreach (var checkpoint in checkpoints)
                {
                    try
                    {
                        var workflow = await recoveryFactory(checkpoint);
                        _workflows.Add(workflow);
                        _logger.LogInformation(
                            $"[调度器] 工作流 '{checkpoint.WorkflowName}' 已恢复，当前步骤: {checkpoint.CurrentStepIndex}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            $"[调度器] 恢复工作流 '{checkpoint.WorkflowName}' 失败");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[调度器] 恢复过程失败");
            }
        }

        public Task StartAsync(IWorkflow workflow, CancellationToken cancellationToken = default)
        {
            _workflows.Add(workflow);
            return workflow.StartAsync(cancellationToken);
        }

        public async Task UpdateAsync(CancellationToken cancellationToken = default)
        {
            for (var i = _workflows.Count - 1; i >= 0; i--)
            {
                await _workflows[i].UpdateAsync(cancellationToken);
                await CheckAndRemoveAt(i, cancellationToken);
            }
        }

        private async Task CheckAndRemoveAt(int index, CancellationToken cancellationToken)
        {
            var workflow = _workflows[index];
            if (workflow.IsFinished)
            {
                // 为完成的工作流清除检查点
                if (workflow is IPersistableWorkflow persistable)
                {
                    // 尝试清除检查点
                    await persistable.ClearCheckpointAsync(cancellationToken);
                }

                _logger.LogInformation($"[调度器] 工作流 '{workflow.Name}' 执行完毕并退出");
                _workflows.RemoveAt(index);
            }
        }

        public IReadOnlyList<IWorkflow> GetActiveWorkflows() => (IReadOnlyList<IWorkflow>)_workflows;
    }
}
