using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Scripts;
using Ddon.Core.Use.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ddon.Schedule;

internal class ScheduleInvokeHandler : INotificationHandler<ScheduleInvokeEventData>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Python _python;
    private readonly ILogger<ScheduleInvokeHandler> _logger;

    public ScheduleInvokeHandler(
        IServiceProvider serviceProvider,
        Python python,
        ILogger<ScheduleInvokeHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _python = python;
        _logger = logger;
    }

    public async Task Handle(ScheduleInvokeEventData data, CancellationToken cancellationToken)
    {
        var schedule = ScheduleData.Schedules[data.ScheduleId];
        await (schedule.ScheduleType switch
        {
            ScheduleType.Plain => Plain(schedule.GetScheduleType(), cancellationToken),
            ScheduleType.Script => Script(schedule.GetScriptValue(), cancellationToken),
            ScheduleType.Method => Method(schedule.GetMethodValue(), cancellationToken),
            _ => Task.CompletedTask,
        });

        _logger.LogInformation("计划任务[{Description}] CRON:[{Cron}] 执行完成 ", schedule.Description, schedule.Cron.ToString());
    }

    private async Task Plain(Type type, CancellationToken cancellationToken)
    {
        var instance = _serviceProvider.GetService(type) as ISchedule
            ?? throw new Exception($"从[ServiceProvider]中找不到[{type.FullName}]类型的对象");

        await instance.InvokeAsync(cancellationToken);
    }

    private async Task Method(MethodValue value, CancellationToken _)
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
