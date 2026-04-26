using Ddon.UniPLC.Models;

namespace Ddon.UniPLC.Abstractions;

/// <summary>
/// PLC 客户端工厂接口
/// </summary>
public interface IPlcClientFactory
{
    /// <summary>
    /// 创建 PLC 客户端
    /// </summary>
    IPlcClient Create(PlcOptions options);
}
