using Ddon.Desktop.Annotations;
using Ddon.Desktop.Bridge;

namespace Test.Desktop.Avalonia.Services;

[BridgeService(Name = "Plc")]
public class PlcReadService
{
    private readonly Dictionary<string, object> _dataStore = new()
    {
        ["DB1.DBX0.0"] = false,
        ["DB1.DBX0.1"] = true,
        ["DB1.DBW2"] = (short)42,
        ["DB1.DBD4"] = 3.14f,
        ["DB1.DBB10"] = (byte)0xFF,
    };

    private readonly IUiBridge _bridge;

    public PlcReadService(IUiBridge bridge)
    {
        _bridge = bridge;
    }

    [BridgeMethod(Name = "ReadPlc")]
    public Task<object> ReadPlc(string address)
    {
        if (_dataStore.TryGetValue(address, out var value))
        {
            return Task.FromResult(value);
        }
        throw new KeyNotFoundException($"Address {address} not found");
    }

    [BridgeMethod(Name = "WritePlc")]
    public async Task WritePlc(PlcWriteRequest request)
    {
        _dataStore[request.Address] = request.Value!;

        await _bridge.PublishAsync(new Events.PlcDataUpdatedEvent
        {
            Address = request.Address,
            Value = request.Value,
            Timestamp = DateTime.UtcNow
        });
    }

    [BridgeMethod(Name = "ListPlcAddresses")]
    public Task<List<string>> ListPlcAddresses()
    {
        return Task.FromResult(_dataStore.Keys.ToList());
    }

    [BridgeMethod(Name = "ReadAllPlc")]
    public Task<Dictionary<string, object>> ReadAllPlc()
    {
        return Task.FromResult(new Dictionary<string, object>(_dataStore));
    }
}
