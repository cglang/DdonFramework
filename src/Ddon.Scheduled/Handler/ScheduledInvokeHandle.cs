using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Reflection;
using Ddon.Scheduled.Event;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job.Handler
{
    internal class ScheduledInvokeHandle : INotificationHandler<ScheduledInvokeEventData>
    {
        private readonly IServiceProvider _serviceProvider;

        public ScheduledInvokeHandle(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(ScheduledInvokeEventData data, CancellationToken cancellationToken)
        {
            var classType = DdonType.GetTypeByName<IScheduled>(data.JobClassName);
            var instance = _serviceProvider.GetRequiredService(classType) ??
                throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var method = DdonType.GetMothodByName(classType, data.JobMethodName);
            await DdonInvoke.InvokeAsync(instance, method);
        }
    }
}
