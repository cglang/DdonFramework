using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace Ddon.Workflow.Persistence
{
    /// <summary>
    /// 从持久化检查点恢复工作流的服务
    /// </summary>
    public class WorkflowRecoveryService : IWorkflowRecoveryService
    {
        private readonly IWorkflowPersistenceStrategy _persistenceStrategy;
        private readonly ILogger<WorkflowRecoveryService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public WorkflowRecoveryService(
            IWorkflowPersistenceStrategy persistenceStrategy,
            ILogger<WorkflowRecoveryService> logger)
        {
            _persistenceStrategy = persistenceStrategy;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<Workflow<TContext>> RecoverWorkflowAsync<TContext>(
            IWorkflowCheckpoint checkpoint,
            Func<string[], TContext, IStep<TContext>[]> stepFactory,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 反序列化上下文
                var contextType = Type.GetType(checkpoint.ContextTypeName);
                if (contextType == null)
                {
                    throw new InvalidOperationException(
                        $"上下文类型 '{checkpoint.ContextTypeName}' 无法解析。 " +
                        "请确保该类型在当前应用程序域中可用。");
                }

                var context = (TContext)JsonSerializer.Deserialize(
                    checkpoint.ContextJson,
                    contextType,
                    _jsonOptions);

                // 使用提供的工厂重建步骤
                var steps = stepFactory(checkpoint.StepTypeNames, context);

                // 创建恢复的工作流
                var workflow = new Workflow<TContext>(
                    checkpoint.WorkflowName,
                    context,
                    steps)
                {
                    Id = checkpoint.WorkflowId
                };

                // 恢复到检查点索引
                workflow.RestoreCheckpoint(checkpoint.CurrentStepIndex);

                _logger.LogInformation(
                    $"[恢复] 工作流 '{checkpoint.WorkflowName}' " +
                    $"已从检查点恢复，当前步骤: {checkpoint.CurrentStepIndex}/{steps.Length}");

                return await Task.FromResult(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"[恢复] 从检查点恢复工作流失败");
                throw;
            }
        }

        public async Task<IWorkflowCheckpoint[]> GetRecoverableCheckpointsAsync(
            CancellationToken cancellationToken = default)
        {
            var checkpoints = await _persistenceStrategy.GetAllCheckpointsAsync(cancellationToken);
            _logger.LogInformation($"[恢复] 找到 {checkpoints.Length} 个可恢复的检查点");
            return checkpoints;
        }
    }
}