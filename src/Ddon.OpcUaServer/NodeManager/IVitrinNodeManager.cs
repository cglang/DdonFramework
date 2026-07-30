using Opc.Ua;

namespace Ddon.OpcUaServer.NodeManager;

/// <summary>
/// 节点管理器接口，提供 OPC UA 地址空间节点的创建、查找、移除和提交变更能力。
/// </summary>
public interface IVitrinNodeManager
{
    /// <summary>ObjectsFolder 根节点。</summary>
    FolderState ObjectsFolder { get; }

    /// <summary>根据路径字符串查找节点。</summary>
    NodeState? FindNode(string nodePath);

    /// <summary>
    /// 获取指定路径下的子节点列表。
    /// nodePath 为 null 或空时返回 ObjectsFolder 下的子节点。
    /// </summary>
    IReadOnlyList<NodeState> GetChildren(string? nodePath = null);

    /// <summary>在指定父节点下创建 Folder 节点。</summary>
    FolderState CreateFolder(BaseObjectState parent, string name);

    /// <summary>创建 Object 节点。</summary>
    BaseObjectState CreateObject(BaseObjectState parent, string name);

    /// <summary>创建 Variable 节点（只读，通过 readFunc 从数据源读取值）。</summary>
    BaseVariableState CreateVariable(BaseObjectState parent, string name, Type dataType, Func<object?> readFunc);

    /// <summary>创建 Variable 节点（可读写，写时调用 writeFunc）。</summary>
    BaseVariableState CreateVariable(BaseObjectState parent, string name, Type dataType,
        Func<object?> readFunc, Action<object?> writeFunc);

    /// <summary>创建 Method 节点。</summary>
    MethodState CreateMethod(BaseObjectState parent, string name,
        Func<ISystemContext, MethodState, CallMethodRequest, CallMethodResult> onCall);

    /// <summary>从地址空间移除节点。</summary>
    bool RemoveNode(string nodePath);

    /// <summary>
    /// 提交地址空间变更，触发 SDK 更新订阅者的 MonitoredItem。
    /// </summary>
    void ApplyChanges();

    /// <summary>获取所有已注册的 INodeProvider。</summary>
    IReadOnlyList<INodeProvider> Providers { get; }
}
