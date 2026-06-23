namespace Ddon.VitrinPLC.Abstractions
{
    /// <summary>
    /// 外部扩展入口：实现此接口以接入自定义 PLC 客户端。
    /// </summary>
    public interface IPlcClientFactory
    {
        IPlcClient Create(string name);
    }
}
