namespace Ddon.Desktop.Core.Protocol;

public class HeartbeatMessage
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
