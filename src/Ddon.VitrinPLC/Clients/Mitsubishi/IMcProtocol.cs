using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.VitrinPLC.Clients.Mitsubishi;

public interface IMcProtocol : IDisposable
{
    bool Connected { get; }
    Task OpenAsync(CancellationToken cancellationToken = default);
    int Close();
    Task<int> SetBitDevice(string iDeviceName, int iSize, int[] iData);
    Task<int> SetBitDevice(McProtocolDeviceType type, int iAddress, int iSize, int[] iData);
    Task<int> GetBitDevice(string iDeviceName, int iSize, int[] oData);
    Task<int> GetBitDevice(McProtocolDeviceType type, int address, int size, int[] data);
    Task<int> WriteDeviceBlock(string iDeviceName, int iSize, int[] iData);
    Task<int> WriteDeviceBlock(McProtocolDeviceType iType, int iAddress, int iSize, int[] iData);
    Task<int> WriteDeviceBlock(McProtocolDeviceType iType, int iAddress, int iSize, byte[] bData);
    Task<byte[]> ReadDeviceBlock(string iDeviceName, int iSize, int[] oData);
    Task<byte[]> ReadDeviceBlock(McProtocolDeviceType iType, int iAddress, int iSize, int[] oData);
    Task<byte[]> ReadDeviceBlock(McProtocolDeviceType iType, int iAddress, int iSize);
    Task<int> SetDevice(string iDeviceName, int iData);
    Task<int> SetDevice(McProtocolDeviceType iType, int iAddress, int iData);
    Task<int> GetDevice(string iDeviceName);
    Task<int> GetDevice(McProtocolDeviceType iType, int iAddress);
}