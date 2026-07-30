namespace Ddon.OpcUaServer.NodeManager;

/// <summary>
/// 节点提供者接口。
/// 每个设备/功能模块（PLC、扫码枪、TCP 设备）实现此接口，
/// 将自己的节点树注册到 OPC UA 地址空间。
/// </summary>
public interface INodeProvider
{
    /// <summary>提供者在 ObjectsFolder 下的根节点名称（如 "PLC"、"BarcodeScanner"）。</summary>
    string RootName { get; }

    /// <summary>
    /// Server 启动/初始化时调用。提供者在此创建自己的节点子树。
    /// </summary>
    void CreateNodes(IVitrinNodeManager nodeManager);

    /// <summary>
    /// 运行时动态添加子节点（新增设备实例）。
    /// </summary>
    void AddChildNode(IVitrinNodeManager nodeManager, string identifier);

    /// <summary>
    /// 运行时移除子节点。
    /// </summary>
    void RemoveChildNode(string identifier);
}
