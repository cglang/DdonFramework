namespace Ddon.UniPLC.Abstractions;

/// <summary>
/// PLC 提供者接口
/// </summary>
public interface IPlcProvider
{
    /// <summary>
    /// 按名称获取 PLC 客户端
    /// </summary>
    IPlcClient GetClient(string name);

    /// <summary>
    /// 按类型获取 PLC 客户端
    /// </summary>
    T GetClient<T>() where T : IPlcClient;
}
