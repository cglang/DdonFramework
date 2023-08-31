using System;
using Ddon.Core.Use.Cronos;

namespace Ddon.Schedule;

internal class ScheduleInvokeData
{
    public ScheduleInvokeData(CronExpression cron, TimeZoneInfo zone, bool inclusive, string className, string methodName)
    {
        Cron = cron;
        Zone = zone;
        Inclusive = inclusive;
        ClassName = className;
        MethodName = methodName;
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

    public string ClassName { get; set; }

    public string MethodName { get; set; }

    public static ScheduleInvokeData? Parse(string content)
    {
        string[] lines = content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        string cron = string.Empty;
        string className = string.Empty;
        string methodName = string.Empty;
        foreach (var item in lines)
        {
            if (item.IsNullOrWhiteSpace()) continue;

            var kv = item.Split("=");
            if (kv.Length != 2) continue;

            switch (kv[0])
            {
                case "Enable":
                    if (kv[1] == "false")
                        return default;
                    break;
                case "Name":
                    break;
                case "Cron":
                    cron = kv[1];
                    break;
                case "Invoke":
                    className = kv[1].Split(".")[0];
                    methodName = kv[1].Split(".")[1];
                    break;
            }
        }

        CronExpression.Parse(cron);

        return new ScheduleInvokeData(CronExpression.Parse(cron), TimeZoneInfo.Local, true, className, methodName);
    }
}
