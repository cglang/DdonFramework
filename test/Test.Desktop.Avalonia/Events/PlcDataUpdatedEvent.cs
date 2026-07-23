using Ddon.Desktop.Core.Annotations;

namespace Test.Desktop.Avalonia.Events;

[BridgeEvent(Name = "plc.data.updated")]
public class PlcDataUpdatedEvent
{
    public string Address { get; set; } = string.Empty;
    public object? Value { get; set; }
    public DateTime Timestamp { get; set; }
}
