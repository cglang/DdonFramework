namespace Ddon.Desktop.Protocol;

public class HeartbeatMessage
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
