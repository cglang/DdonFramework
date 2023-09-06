using System;
using Ddon.Core.Use.Cronos;

namespace Ddon.Schedule;

internal class ScheduleInvokeData
{
    public ScheduleInvokeData(
        CronExpression cron,
        TimeZoneInfo zone,
        bool inclusive,
        Type type)
    {
        Cron = cron;
        Zone = zone;
        Inclusive = inclusive;
        Type = type;
    }

    public CronExpression Cron { get; set; }

    public TimeZoneInfo Zone { get; }

    public TimeSpan NextSpan
    {
        get
        {
            var now = DateTime.UtcNow;
            var nextOccurrence = Cron.GetNextOccurrence(now, Zone, Inclusive);

            if (nextOccurrence is null || nextOccurrence < now) throw new Exception();

            return (nextOccurrence - now).Value;
        }
    }

    public bool Inclusive { get; }

    public Type Type { get; set; }
}
