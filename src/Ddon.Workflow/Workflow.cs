using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Abstractions.Persistence;
using Ddon.Workflow.Persistence;

namespace Ddon.Workflow
{
    /// <summary>
    /// 串行执行的工作流引擎 负责按顺序驱动一组 Step
    /// </summary>
    public class Workflow<TContext> : WorkflowBase, IPersistableWorkflow
    {
        protected readonly IList<IStep<TContext>> _steps;
        private readonly IList<IStepExtension<TContext>> _extensions = new List<IStepExtension<TContext>>();
        private IWorkflowPersistenceStrategy _persistenceStrategy;
        private bool _isPersistenceEnabled;
        private int _lastPersistedStepIndex = -1;

        /// <summary>
        /// 工作流上下文：包含执行该工作流所需的所有数据和状态
        /// </summary>
        public TContext Context { get; private set; }

        [JsonIgnore]
        public IWorkflowPersistenceStrategy PersistenceStrategy
        {
            get => _persistenceStrategy;
            set => _persistenceStrategy = value;
        }

        [JsonIgnore]
        public bool IsPersistenceEnabled
        {
            get => _isPersistenceEnabled;
            set => _isPersistenceEnabled = value;
        }

        /// <summary>
        /// 串行执行的工作流引擎 负责按顺序驱动一组 Step
        /// </summary>
        /// <param name="name">工作流名称</param>
        /// <param name="steps">工作流步骤</param>
        public Workflow(string name, TContext context, IList<IStep<TContext>> steps) : base(steps)
        {
            _steps = steps;
            Name = name;
            Context = context;
            Id = Guid.NewGuid().ToString();
            _isPersistenceEnabled = false;
        }

        private IEnumerable<IStepExtension<TContext>> GetExtensionsForStep(IStep<TContext> step)
        {
            var stepExtensions = Enumerable.Empty<IStepExtension<TContext>>();
            if (step is Step<TContext> s)
            {
                stepExtensions = s.Extensions;
            }

            return _extensions.Concat(stepExtensions);
        }

        /// <summary>
        /// 为工作流注册步骤扩展点
        /// </summary>
        public Workflow<TContext> AddExtension(IStepExtension<TContext> extension)
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            _extensions.Add(extension);
            return this;
        }

        /// <summary>
        /// 启用此工作流的持久化
        /// </summary>
        public void EnablePersistence(IWorkflowPersistenceStrategy persistenceStrategy)
        {
            _persistenceStrategy = persistenceStrategy
                ?? throw new ArgumentNullException(nameof(persistenceStrategy));
            _isPersistenceEnabled = true;
        }

        /// <summary>
        /// 开始执行工作流 从第一个步骤开始
        /// </summary>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _steps[0].OnEnterAsync(Context, cancellationToken);
            // 当首个步骤进入完成后，触发扩展点（异步、不阻塞启动流程）
            foreach (var ext in GetExtensionsForStep(_steps[0]))
            {
                try
                {
                    await ext.AfterEnterAsync(_steps[0], Context, cancellationToken).ConfigureAwait(false);
                }
                catch { /* 扩展异常吞掉，避免影响流程启动 */ }
            }
        }

        /// <summary>
        /// 每帧更新（驱动执行） 根据当前步骤的状态决定是否进入下一步骤
        /// </summary>
        public override async Task UpdateAsync(CancellationToken cancellationToken)
        {
            if (IsFinished) return;

            var step = _steps[_index];
            var result = await step.OnUpdateAsync(Context, cancellationToken);

            if (result == StepStatus.Success)
            {
                await step.OnExitAsync(Context, cancellationToken);

                // 在步骤退出后触发扩展点
                foreach (var ext in GetExtensionsForStep(step))
                {
                    try
                    {
                        await ext.AfterExitAsync(step, Context, cancellationToken).ConfigureAwait(false);
                    }
                    catch { }
                }

                // 当前步骤成功，跳转索引
                _index++;

                if (!IsFinished)
                {
                    // 启动下一步骤
                    await _steps[_index].OnEnterAsync(Context, cancellationToken);

                    // 下一步骤进入后触发扩展点（仅触发该步骤对应的扩展以及工作流级扩展）
                    foreach (var ext in GetExtensionsForStep(_steps[_index]))
                    {
                        try
                        {
                            await ext.AfterEnterAsync(_steps[_index], Context, cancellationToken).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }

                // 步骤成功转换后创建检查点
                if (_isPersistenceEnabled)
                {
                    await CheckpointAsync(cancellationToken);
                }
            }
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
        /// 将当前状态持久化到存储
        /// </summary>
        public async Task CheckpointAsync(CancellationToken cancellationToken = default)
        {
            if (!_isPersistenceEnabled || _persistenceStrategy == null)
                return;

            // 仅在步骤索引发生变化时持久化
            if (_lastPersistedStepIndex == _index)
                return;

            try
            {
                var checkpoint = CreateCheckpoint();
                await _persistenceStrategy.SaveCheckpointAsync(checkpoint, cancellationToken);
                _lastPersistedStepIndex = _index;
            }
            catch (Exception ex)
            {
                // 记录但不失败 - 持久化是非关键的
                System.Diagnostics.Debug.WriteLine($"[工作流] 工作流 {Id} 检查点失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 恢复工作流到之前检查点的步骤索引
        /// </summary>
        public void RestoreCheckpoint(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _steps.Count)
                throw new ArgumentOutOfRangeException(nameof(stepIndex));

            _index = stepIndex;
            _lastPersistedStepIndex = stepIndex;
        }

        /// <summary>
        /// 工作流完成后清除持久化检查点
        /// </summary>
        public async Task ClearCheckpointAsync(CancellationToken cancellationToken = default)
        {
            if (!_isPersistenceEnabled || _persistenceStrategy == null)
                return;

            try
            {
                await _persistenceStrategy.DeleteCheckpointAsync(Id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[工作流] 删除工作流 {Id} 的检查点失败: {ex.Message}");
            }
        }
    }
}
