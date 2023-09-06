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

    public Task Handle(ScheduleInvokeEventData data, CancellationToken cancellationToken)
    {
        var schedule = ScheduleData.Schedules[data.ScheduleId];

        return schedule.ScheduleType switch
        {
            ScheduleType.Plain => Plain(schedule.GetScheduleType(), cancellationToken),
            ScheduleType.Script => Script(schedule.GetScriptValue(), cancellationToken),
            ScheduleType.Method => Method(schedule.GetMethodValue(), cancellationToken),
            _ => Task.CompletedTask,
        };
    }

    private async Task Plain(Type type, CancellationToken cancellationToken)
    {
        var instance = _serviceProvider.GetService(type) as ISchedule
            ?? throw new Exception($"从[ServiceProvider]中找不到[{type.FullName}]类型的对象");

        await instance.InvokeAsync(cancellationToken);
    }

    private async Task Method(MethodValue value, CancellationToken cancellationToken)
    {
        var classType = DdonType.GetTypeByName<ISchedule>(value.ClassName);
        var instance = _serviceProvider.GetRequiredService(classType) ??
            throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

        var method = DdonType.GetMothodByName(classType, value.MethodName);
        await DdonInvoke.InvokeAsync(instance, method);
    }

    private Task Script(ScriptValue value, CancellationToken cancellationToken)
    {
        if (value.ScriptType is ScriptType.Python)
            _python.Run(value.ScriptPath);

        return Task.FromResult(cancellationToken);
    }
}

internal class ScheduleInvokeEventData : INotification
{
    public ScheduleInvokeEventData(Guid scheduleId)
    {
        ScheduleId = scheduleId;
    }

    public Guid ScheduleId { get; set; }
}
