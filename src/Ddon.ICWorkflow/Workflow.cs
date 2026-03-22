using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.ICWorkflow
{
    public abstract class Workflow
    {
        protected int _index;

        protected IEnumerable<IStep> BaseSteps;

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 工作流是否已完成（所有步骤都成功）
        /// </summary>
        public bool IsFinished => _index >= BaseSteps.Count();

        public abstract void Start();

        public abstract void Update();
    }

    /// <summary>
    /// 串行执行的工作流引擎 负责按顺序驱动一组 Step
    /// </summary>
    public class Workflow<TContext> : Workflow
    {
        protected readonly List<IStep<TContext>> _steps;

        /// <summary>
        /// 工作流上下文：包含执行该工作流所需的所有数据和状态
        /// </summary>
        public TContext Context { get; private set; }

        /// <summary>
        /// 串行执行的工作流引擎 负责按顺序驱动一组 Step
        /// </summary>
        /// <param name="name">工作流名称</param>
        /// <param name="steps">工作流步骤</param>
        public Workflow(string name, TContext context, List<IStep<TContext>> steps)
        {
            Name = name;
            Context = context;
            BaseSteps = steps.Select(x=>x as IStep);
            _steps = steps;
        }

        /// <summary>
        /// 开始执行工作流 从第一个步骤开始
        /// </summary>
        public override void Start() => _steps[0].OnEnter(Context);

        /// <summary>
        /// 每帧更新（驱动执行） 根据当前步骤的状态决定是否进入下一步骤
        /// </summary>
        public override void Update()
        {
            if (IsFinished) return;

            var step = _steps[_index];
            var result = step.OnUpdate(Context);

            if (result == StepStatus.Success)
            {
                step.OnExit(Context);

                _index++; // 当前步骤成功，跳转索引

                if (!IsFinished)
                    _steps[_index].OnEnter(Context); // 启动下一步骤
            }
        }
    }
}
