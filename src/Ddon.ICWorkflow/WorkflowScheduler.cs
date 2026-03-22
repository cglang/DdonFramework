using System;
using System.Collections.Generic;

namespace Ddon.ICWorkflow
{
    public class WorkflowScheduler
    {
        private readonly List<Workflow> _workflows = new List<Workflow>();

        public void Start(Workflow wf)
        {
            wf.Start();
            _workflows.Add(wf);
        }

        public void Update()
        {
            for (int i = _workflows.Count - 1; i >= 0; i--)
            {
                _workflows[i].Update();
                if (_workflows[i].IsFinished)
                {
                    Console.WriteLine($"[调度] {_workflows[i].Name} 的流执行完毕并退出。");
                    _workflows.RemoveAt(i);
                }
            }
        }
    }
}
