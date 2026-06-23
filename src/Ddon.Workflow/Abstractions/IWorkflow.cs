using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Workflow.Abstractions
{
    public interface IWorkflow
    {
        string Id { get; set; }

        string Name { get; }

        bool IsFinished { get; }

        Task StartAsync(CancellationToken cancellationToken);

        Task UpdateAsync(CancellationToken cancellationToken);
    }

    public abstract class WorkflowBase : IWorkflow
    {
        protected int _index;

        private readonly IEnumerable<IStep> _steps;

        protected WorkflowBase(IEnumerable<IStep> steps)
        {
            _steps = steps;
        }

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 工作流是否已完成（所有步骤都成功）
        /// </summary>
        public bool IsFinished => _index >= _steps.Count();

        public abstract Task StartAsync(CancellationToken cancellationToken);

        public abstract Task UpdateAsync(CancellationToken cancellationToken = default);
    }
}
