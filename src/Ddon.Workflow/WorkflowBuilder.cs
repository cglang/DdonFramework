using System;
using System.Collections.Generic;
using Ddon.Workflow.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Workflow
{
    public class WorkflowBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public WorkflowBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public WorkflowBuilder<TContext> CreateWorkflow<TContext>()
        {
            return new WorkflowBuilder<TContext>(_serviceProvider);
        }
    }

    public class WorkflowBuilder<TContext>
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IList<IStep<TContext>> _steps = new List<IStep<TContext>>();

        public WorkflowBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public WorkflowBuilder<TContext> AddStep<TStep>() where TStep : IStep<TContext>
        {
            var step = _serviceProvider.GetRequiredService<TStep>();
            _steps.Add(step);
            return this;
        }

        public WorkflowBuilder<TContext> AddStep<TStep, TStepExtension>()
            where TStep : IStep<TContext>
            where TStepExtension : IStepExtension<TContext>
        {
            var step = _serviceProvider.GetRequiredService<TStep>();
            var stepExtension = _serviceProvider.GetRequiredService<TStepExtension>();
            step.AddExtension(stepExtension);
            _steps.Add(step);
            return this;
        }

        public WorkflowBuilder<TContext> AddStep(IStep<TContext> step)
        {
            if (step != null)
                _steps.Add(step);
            return this;
        }

        public WorkflowBuilder<TContext> AddStep(IStep<TContext> step, IStepExtension<TContext> stepExtension)
        {
            if (step != null)
            {
                if (stepExtension != null)
                    step.AddExtension(stepExtension);
                _steps.Add(step);
            }
            return this;
        }

        public Workflow<TContext> Build(string name, TContext context)
        {
            return new Workflow<TContext>(name, context, _steps);
        }
    }
}
