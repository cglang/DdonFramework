using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Workflow.Abstractions.Persistence
{
    /// <summary>
    /// 表示工作流的持久化检查点
    /// </summary>
    public interface IWorkflowCheckpoint
    {
        /// <summary>
        /// 工作流实例唯一标识符
        /// </summary>
        string WorkflowId { get; }

        /// <summary>
        /// 工作流名称
        /// </summary>
        string WorkflowName { get; }

        /// <summary>
        /// 当前步骤索引
        /// </summary>
        int CurrentStepIndex { get; }

        /// <summary>
        /// 序列化的上下文对象（JSON）
        /// </summary>
        string ContextJson { get; }

        /// <summary>
        /// 上下文类型的完整名称
        /// </summary>
        string ContextTypeName { get; }

        /// <summary>
        /// 步骤类型信息，用于重建步骤列表
        /// </summary>
        string[] StepTypeNames { get; }

        /// <summary>
        /// 检查点创建时间戳
        /// </summary>
        DateTime CreatedAt { get; }

        /// <summary>
        /// 当前工作流状态
        /// </summary>
        string Status { get; }
    }

    /// <summary>
    /// 工作流持久化策略的策略模式接口
    /// </summary>
    public interface IWorkflowPersistenceStrategy
    {
        /// <summary>
        /// 保存工作流检查点
        /// </summary>
        Task SaveCheckpointAsync(
            IWorkflowCheckpoint checkpoint,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 按ID加载工作流检查点
        /// </summary>
        Task<IWorkflowCheckpoint> LoadCheckpointAsync(
            string workflowId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有可用的检查点（用于启动时恢复）
        /// </summary>
        Task<IWorkflowCheckpoint[]> GetAllCheckpointsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除检查点（工作流完成后）
        /// </summary>
        Task DeleteCheckpointAsync(
            string workflowId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查指定工作流的检查点是否存在
        /// </summary>
        Task<bool> CheckpointExistsAsync(
            string workflowId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 从持久化检查点恢复工作流的服务
    /// </summary>
    public interface IWorkflowRecoveryService
    {
        /// <summary>
        /// 从检查点恢复工作流并继续执行
        /// </summary>
        /// <typeparam name="TContext">工作流的上下文类型</typeparam>
        /// <param name="checkpoint">要恢复的检查点</param>
        /// <param name="stepFactory">用于从类型名称重建步骤的工厂方法</param>
        /// <returns>恢复的工作流实例</returns>
        Task<Workflow<TContext>> RecoverWorkflowAsync<TContext>(
            IWorkflowCheckpoint checkpoint,
            Func<string[], TContext, IStep<TContext>[]> stepFactory,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 加载所有可恢复的检查点
        /// </summary>
        Task<IWorkflowCheckpoint[]> GetRecoverableCheckpointsAsync(
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 支持持久化的工作流标记接口
    /// </summary>
    public interface IPersistableWorkflow
    {
        /// <summary>
        /// 创建当前状态的检查点
        /// </summary>
        IWorkflowCheckpoint CreateCheckpoint();

        Task ClearCheckpointAsync(CancellationToken cancellationToken = default);
    }
}
