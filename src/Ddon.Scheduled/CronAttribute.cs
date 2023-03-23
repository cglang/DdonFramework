using System;
using Ddon.Core.Use.Cronos;

namespace Ddon.Scheduled;

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
    public TimeZoneInfo Zone { get; } = TimeZoneInfo.Local;
    public bool Inclusive { get; }
    public bool Enable { get; }
}
