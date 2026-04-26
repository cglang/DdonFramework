using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Models;

namespace Ddon.UniPLC.Clients.Siemens;

/// <summary>
/// Siemens PLC 客户端工厂
/// </summary>
public class SiemensPlcClientFactory : IPlcClientFactory
{
    public IPlcClient Create(PlcOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var siemensOptions = options as SiemensPlcOptions 
            ?? new SiemensPlcOptions
            {
                Name = options.Name,
                Ip = options.Ip,
                Port = options.Port,
                ConnectTimeout = options.ConnectTimeout,
                OperationTimeout = options.OperationTimeout,
                ReconnectInterval = options.ReconnectInterval
            };

        return new SiemensPlcClient(siemensOptions);
    }
}
