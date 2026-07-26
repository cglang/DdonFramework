using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.VitrinPLC.Clients.Mitsubishi;

public interface IMcProtocol : IDisposable
{
    bool Connected { get; }
    
    Task OpenAsync(CancellationToken cancellationToken = default);
    int Close();
    
    Task<int> WriteDeviceBlock(McProtocolDeviceType type, int address, int size, byte[] data);
    
    Task<byte[]> ReadDeviceBlock(McProtocolDeviceType type, int address, int size);
}