using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;

namespace Ddon.Workflow.Persistence
{
    /// <summary>
    /// 支持持久化的工作流：在步骤成功推进后自动保存检查点，完成后自动清除
    /// 核心 Workflow 不感知持久化，此子类通过生命周期钩子挂载持久化行为
    /// </summary>
    public class PersistableWorkflow<TContext> : Workflow<TContext>
    {
        private readonly IWorkflowPersistenceStrategy _strategy;
        private int _lastPersistedStepIndex;

        /// <summary>
        /// 创建支持持久化的工作流
        /// </summary>
        /// <param name="name">工作流名称</param>
        /// <param name="context">工作流上下文</param>
        /// <param name="steps">工作流步骤</param>
        /// <param name="strategy">持久化策略</param>
        /// <param name="startIndex">起始步骤索引（用于从中途恢复，默认从第一个步骤开始）</param>
        public PersistableWorkflow(
            string name,
            TContext context,
            IList<IStep<TContext>> steps,
            IWorkflowPersistenceStrategy strategy,
            int startIndex = 0) : base(name, context, steps, startIndex)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _lastPersistedStepIndex = startIndex;
        }

        /// <summary>
        /// 创建当前工作流状态的检查点
        /// </summary>
        public IWorkflowCheckpoint CreateCheckpoint()
        {
            var contextJson = JsonSerializer.Serialize(
                Context,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            return new WorkflowCheckpoint
            {
                WorkflowId = Id,
                WorkflowName = Name,
                CurrentStepIndex = _index,
                ContextJson = contextJson,
                ContextTypeName = typeof(TContext).FullName,
                StepTypeNames = _steps.Select(s => s.GetType().FullName).ToArray(),
                Status = IsFinished ? "Completed" : "Running",
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 步骤成功推进后保存检查点
        /// </summary>
        protected override async Task OnStepAdvancedAsync(CancellationToken cancellationToken)
        {
            // 仅在步骤索引发生变化时持久化
            if (_lastPersistedStepIndex == _index)
                return;

            try
            {
                var checkpoint = CreateCheckpoint();
                await _strategy.SaveCheckpointAsync(checkpoint, cancellationToken);
                _lastPersistedStepIndex = _index;
            }
            catch (Exception ex)
            {
                // 记录但不失败 - 持久化是非关键的
                Debug.WriteLine($"[工作流] 工作流 {Id} 检查点失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 工作流完成后清除持久化检查点
        /// </summary>
        protected override async Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _strategy.DeleteCheckpointAsync(Id, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[工作流] 删除工作流 {Id} 的检查点失败: {ex.Message}");
            }
        }
    }
}
