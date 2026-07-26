using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.AddressParsers;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Clients.Mitsubishi;

public sealed class McProtocolClient : IPlcClient
{
    private readonly McProtocol _protocol;

    public string Name { get; }

    public bool IsConnected => _protocol.Connected;
    public IPlcAddressParser Parser { get; } = new MitsubishiAddressParser();

    public McProtocolClient(string name, string host, int port, McProtocolFrame frame)
    {
        Name = name;
        _protocol = new McProtocol(host, port, frame);
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _protocol.OpenAsync(ct);
    }

    public Task DisconnectAsync(CancellationToken ct = default)
    {
        _protocol.Close();
        return Task.CompletedTask;
    }

    public async Task<byte[]> ReadBytesAsync(string area, int start, int length, CancellationToken ct = default)
    {
        var deviceType = McProtocolBase.GetDeviceType(area);
        var isBit = McProtocolBase.IsBitDevice(deviceType);

        if (isBit)
        {
            var deviceAddress = start * 8;
            var devicePoints = length * 8;
            return await _protocol.ReadDeviceBlock(deviceType, deviceAddress, devicePoints);
        }

        return await _protocol.ReadDeviceBlock(deviceType, start / 2, length);
    }

    public async Task WriteBytesAsync(string address, byte[] data, CancellationToken ct = default)
    {
        var addr = Parser.Parse(address, PlcDataType.Int16);
        var deviceType = McProtocolBase.GetDeviceType(addr.Area);

        var devicePoints = data.Length / 2;
        var deviceAddress = addr.ByteOffset / 2;

        await _protocol.WriteDeviceBlock(deviceType, deviceAddress, devicePoints, data);
    }

    public void Dispose()
    {
        _protocol.Dispose();
    }
}