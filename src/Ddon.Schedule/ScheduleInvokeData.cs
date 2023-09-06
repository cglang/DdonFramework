using System;
using Ddon.Core.Use.Cronos;

namespace Ddon.Schedule;

internal class ScheduleInvokeData
{
    public ScheduleInvokeData(CronExpression cron, TimeZoneInfo zone, bool inclusive, Type type)
    {
        Cron = cron;
        Zone = zone;
        Inclusive = inclusive;
        Value = type;
        ScheduleType = ScheduleType.Plain;
    }

    public ScheduleInvokeData(CronExpression cron, TimeZoneInfo zone, bool inclusive, string className, string methodName)
    {
        Cron = cron;
        Zone = zone;
        Inclusive = inclusive;
        Value = new MethodValue(className, methodName);
        ScheduleType = ScheduleType.Method;
    }

    public ScheduleInvokeData(CronExpression cron, TimeZoneInfo zone, bool inclusive, ScriptValue scriptValue)
    {
        Cron = cron;
        Zone = zone;
        Inclusive = inclusive;
        Value = scriptValue;
        ScheduleType = ScheduleType.Script;
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

    public ScheduleType ScheduleType { get; }

    public object Value { get; set; }

    public Type GetScheduleType()
    {
        return Value.As<Type>();
    }

    public MethodValue GetMethodValue()
    {
        return Value.As<MethodValue>();
    }

    public ScriptValue GetScriptValue()
    {
        return Value.As<ScriptValue>();
    }

    public static ScheduleInvokeData? Parse(string content)
    {
        string[] lines = content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        string cron = string.Empty;
        string path = string.Empty;
        ScriptType type = 0;
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
                case "Path":
                    path = kv[1];
                    break;
                case "Type":
                    if (kv[1] == "py" || kv[1] == "python")
                        type = ScriptType.Python;
                    if (kv[1] == "js")
                        type = ScriptType.Node;
                    break;
            }
        }

        var scriptValue = new ScriptValue(type, path);

        return new ScheduleInvokeData(CronExpression.Parse(cron, CronFormat.IncludeSeconds), TimeZoneInfo.Local, true, scriptValue);
    }
}

public enum ScheduleType
{
    Plain,
    Script,
    Method
}

public enum ScriptType
{
    Python,
    Node
}

public class MethodValue
{
    public MethodValue(string className, string methodName)
    {
        ClassName = className;
        MethodName = methodName;
    }

    public string ClassName { get; set; }

    public string MethodName { get; set; }
}

public class ScriptValue
{
    public ScriptValue(ScriptType scriptType, string scriptPath)
    {
        ScriptType = scriptType;
        ScriptPath = scriptPath;
    }

    public ScriptType ScriptType { get; set; }

    public string ScriptPath { get; set; }
}
