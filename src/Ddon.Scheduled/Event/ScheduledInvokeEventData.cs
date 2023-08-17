using Ddon.EventBus;

namespace Ddon.Scheduled.Event;

internal class ScheduledInvokeEventData : IEventData
{
    public ScheduledInvokeEventData(string jobClassName, string jobMethodName)
    {
        JobClassName = jobClassName;
        JobMethodName = jobMethodName;
    }

    public string JobClassName { get; set; }

    public string JobMethodName { get; set; }
}
