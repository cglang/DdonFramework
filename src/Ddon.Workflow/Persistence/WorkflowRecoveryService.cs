using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;
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
                var contextType = ResolveType(checkpoint.ContextTypeName);
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

                // 创建带持久化支持的工作流，并恢复到检查点索引
                var workflow = new PersistableWorkflow<TContext>(
                    checkpoint.WorkflowName,
                    context,
                    steps,
                    _persistenceStrategy,
                    checkpoint.CurrentStepIndex)
                {
                    Id = checkpoint.WorkflowId
                };

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

        /// <summary>
        /// 解析类型名：支持程序集限定名；纯全名时在当前已加载程序集中查找
        /// </summary>
        private static Type ResolveType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
