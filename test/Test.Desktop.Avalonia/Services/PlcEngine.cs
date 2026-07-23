using Ddon.Desktop.Core.Bridge;
using Test.Desktop.Avalonia.Events;

namespace Test.Desktop.Avalonia.Services;

public class PlcEngine
{
    private readonly IUiBridge _bridge;
    private Timer? _timer;
    private int _counter;

    public PlcEngine(IUiBridge bridge)
    {
        _bridge = bridge;
    }

    public void Start()
    {
        _timer = new Timer(OnTick, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(3));
    }

    public void Stop()
    {
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();
        _timer = null;
    }

    private async void OnTick(object? state)
    {
        _counter++;
        try
        {
            await _bridge.PublishAsync(new PlcDataUpdatedEvent
            {
                Address = $"DB1.DBD{4 + (_counter % 3) * 4}",
                Value = Random.Shared.Next(0, 1000) / 100.0f,
                Timestamp = DateTime.UtcNow
            });
        }
        catch { }
    }
}
