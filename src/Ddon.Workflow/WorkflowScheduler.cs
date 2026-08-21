using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow.Abstractions;
using Microsoft.Extensions.Logging;

namespace Ddon.Workflow
{
    /// <summary>
    /// 工作流调度器
    /// </summary>
    public class WorkflowScheduler : IWorkflowScheduler
    {
        private readonly IList<IWorkflow> _workflows;
        private readonly ILogger<IWorkflowScheduler> _logger;

        public WorkflowScheduler(ILogger<IWorkflowScheduler> logger)
        {
            _workflows = new List<IWorkflow>();
            _logger = logger;
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
                CheckAndRemoveAt(i);
            }
        }

        private void CheckAndRemoveAt(int index)
        {
            var workflow = _workflows[index];
            if (workflow.IsFinished)
            {
                _logger.LogInformation($"[调度器] 工作流 '{workflow.Name}' 执行完毕并退出");
                _workflows.RemoveAt(index);
            }
        }

        public IReadOnlyList<IWorkflow> GetActiveWorkflows() => (IReadOnlyList<IWorkflow>)_workflows;
    }
}
