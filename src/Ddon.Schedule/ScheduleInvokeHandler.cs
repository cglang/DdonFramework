using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Scripts;
using Ddon.Core.Use.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Schedule;

internal class ScheduleInvokeHandler : INotificationHandler<ScheduleInvokeEventData>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Python _python;

    public ScheduleInvokeHandler(IServiceProvider serviceProvider, Python python)
    {
        _serviceProvider = serviceProvider;
        _python = python;
    }

    public async Task Handle(ScheduleInvokeEventData data, CancellationToken cancellationToken)
    {
        var schedule = ScheduleData.Schedules[data.ScheduleId];

        if (schedule.ScheduleType is ScheduleType.Plain)
        {
            var type = schedule.GetScheduleType();
            var instance = _serviceProvider.GetService(type) as ISchedule
                ?? throw new Exception($"从[ServiceProvider]中找不到[{type.FullName}]类型的对象");

            await instance.InvokeAsync(cancellationToken);
        }
        else if (schedule.ScheduleType is ScheduleType.Method)
        {
            var value = schedule.GetMethodValue();
            var classType = DdonType.GetTypeByName<ISchedule>(value.ClassName);
            var instance = _serviceProvider.GetRequiredService(classType) ??
                throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var method = DdonType.GetMothodByName(classType, value.MethodName);
            await DdonInvoke.InvokeAsync(instance, method);
        }
        else if (schedule.ScheduleType is ScheduleType.Script)
        {
            var value = schedule.GetScriptValue();
            _python.Run(value.ScriptPath);
        }
    }
}

internal class ScheduleInvokeEventData : INotification
{
    public ScheduleInvokeEventData(Guid scheduleId)
    {
        ScheduleId = scheduleId;
    }

    //public Type ScheduleType { get; set; }

    public Guid ScheduleId { get; set; }
}
