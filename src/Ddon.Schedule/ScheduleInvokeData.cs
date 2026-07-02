using System;
using Cronos;

namespace Ddon.Schedule;

public class ScheduleInvokeData
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

    public string? Description { get; set; }

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
