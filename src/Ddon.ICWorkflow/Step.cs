namespace Ddon.ICWorkflow
{
    public interface IStep
    {
    }

    public interface IStep<TContext> : IStep
    {
        /// <summary>
        /// 步骤名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 进入该步骤时的初始化（仅执行一次）
        /// </summary>
        void OnEnter(TContext context);

        /// <summary>
        /// 步骤执行中的逻辑（轮询执行）
        /// </summary>
        StepStatus OnUpdate(TContext context);

        /// <summary>
        /// 步骤完成后的清理（仅执行一次）
        /// </summary>
        void OnExit(TContext context);
    }

    /// <summary>
    /// 抽象步骤基类：所有的动作或逻辑都继承此类
    /// </summary>
    public abstract class Step<TContext> : IStep<TContext>
    {
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 进入该步骤时的初始化（仅执行一次）
        /// </summary>
        public virtual void OnEnter(TContext context) { }

        /// <summary>
        /// 步骤执行中的逻辑（轮询执行）
        /// </summary>
        public abstract StepStatus OnUpdate(TContext context);

        /// <summary>
        /// 步骤完成后的清理（仅执行一次）
        /// </summary>
        public virtual void OnExit(TContext context) { }
    }
}
