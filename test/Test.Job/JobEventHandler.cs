using Ddon.Job;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gardener.Domain.Handler.Job
{
    public class JobEventHandler : INotificationHandler<JobEventData>
    {
        public JobEventHandler() { }

        public async Task Handle(JobEventData @event, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{@event.Id}--{DateTime.Now:HH:mm:ss:ffff}");
            await Task.CompletedTask;
        }
    }
}
