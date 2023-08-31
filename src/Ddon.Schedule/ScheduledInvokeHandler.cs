using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Reflection;
using Ddon.EventBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Schedule;

internal class ScheduledInvokeHandler : INotificationHandler<ScheduleInvokeEventData>
{
    private readonly IServiceProvider _serviceProvider;

    public ScheduledInvokeHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(ScheduleInvokeEventData data, CancellationToken cancellationToken)
    {
        var classType = DdonType.GetTypeByName<ISchedule>(data.JobClassName);
        var instance = _serviceProvider.GetRequiredService(classType) ??
            throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

        var method = DdonType.GetMothodByName(classType, data.JobMethodName);
        await DdonInvoke.InvokeAsync(instance, method);
    }
}

internal class ScheduleInvokeEventData : IEventData
{
    public ScheduleInvokeEventData(string jobClassName, string jobMethodName)
    {
        JobClassName = jobClassName;
        JobMethodName = jobMethodName;
    }

    public string JobClassName { get; set; }

    public string JobMethodName { get; set; }
}
