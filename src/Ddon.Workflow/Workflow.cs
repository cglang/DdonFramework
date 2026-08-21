using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;

namespace Ddon.Workflow
{
    /// <summary>
    /// 串行执行的工作流引擎 负责按顺序驱动一组 Step
    /// </summary>
    public class Workflow<TContext> : WorkflowBase
    {
        protected readonly IList<IStep<TContext>> _steps;
        private readonly IList<IStepExtension<TContext>> _extensions = new List<IStepExtension<TContext>>();

        /// <summary>
        /// 工作流上下文：包含执行该工作流所需的所有数据和状态
        /// </summary>
        public TContext Context { get; private set; }

        /// <summary>
        /// 串行执行的工作流引擎 负责按顺序驱动一组 Step
        /// </summary>
        /// <param name="name">工作流名称</param>
        /// <param name="context">工作流上下文</param>
        /// <param name="steps">工作流步骤</param>
        /// <param name="startIndex">起始步骤索引（用于从中途恢复，默认从第一个步骤开始）</param>
        public Workflow(string name, TContext context, IList<IStep<TContext>> steps, int startIndex = 0) : base(steps)
        {
            if (startIndex < 0 || startIndex > steps.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            _steps = steps;
            Name = name;
            Context = context;
            Id = Guid.NewGuid().ToString();
            _index = startIndex;
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

                // 生命周期钩子：步骤推进成功后触发（完成时触发完成钩子，否则触发推进钩子）
                if (IsFinished)
                {
                    await OnWorkflowCompletedAsync(cancellationToken);
                }
                else
                {
                    await OnStepAdvancedAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// 步骤成功推进后的生命周期钩子（默认空实现，供派生类扩展，如持久化检查点）
        /// </summary>
        protected virtual Task OnStepAdvancedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 工作流完成后的生命周期钩子（默认空实现，供派生类扩展，如清理检查点）
        /// </summary>
        protected virtual Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
