using System;
using Ddon.Core.Use.Cronos;

namespace Ddon.Schedule;

[AttributeUsage(AttributeTargets.Method)]
public class CronAttribute : Attribute
{
    public CronAttribute(string cronExpression, CronFormat format = CronFormat.IncludeSeconds, bool inclusive = false, bool enable = true)
    {
        CronExpression = cronExpression;
        Format = format;
        Inclusive = inclusive;
        Enable = enable;
    }

    public string CronExpression { get; }

    public CronFormat Format { get; }

    public bool Inclusive { get; }

    public bool Enable { get; }
}
