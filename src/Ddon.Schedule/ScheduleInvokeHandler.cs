using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Ddon.Schedule;

internal class ScheduleInvokeHandler : INotificationHandler<ScheduleInvokeEventData>
{
    private readonly IServiceProvider _serviceProvider;

    public ScheduleInvokeHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(ScheduleInvokeEventData data, CancellationToken cancellationToken)
    {
        var instance = _serviceProvider.GetService(data.ScheduleType) as ISchedule
            ?? throw new Exception($"从[ServiceProvider]中找不到[{data.ScheduleType.FullName}]类型的对象");

        await instance.InvokeAsync(cancellationToken);
    }
}

internal class ScheduleInvokeEventData : INotification
{
    public ScheduleInvokeEventData(Type scheduleType)
    {
        ScheduleType = scheduleType;
    }

    public Type ScheduleType { get; set; }
}
