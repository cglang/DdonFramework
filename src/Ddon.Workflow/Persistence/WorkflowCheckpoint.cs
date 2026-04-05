using System;
using Ddon.Workflow.Abstractions.Persistence;

namespace Ddon.Workflow.Persistence
{
    /// <summary>
    /// IWorkflowCheckpoint的默认实现
    /// </summary>
    public class WorkflowCheckpoint : IWorkflowCheckpoint
    {
        public string WorkflowId { get; set; }
        public string WorkflowName { get; set; }
        public int CurrentStepIndex { get; set; }
        public string ContextJson { get; set; }
        public string ContextTypeName { get; set; }
        public string[] StepTypeNames { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }

        public WorkflowCheckpoint()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}