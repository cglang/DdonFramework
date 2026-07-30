using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Server;
using Ddon.OpcUaServer.Nodes;

namespace Ddon.OpcUaServer.NodeManager;

/// <summary>
/// OPC UA 节点管理器实现。
/// 管理地址空间中的节点树创建、查找、移除，以及 INodeProvider 的注册。
/// 遵循 Gateway.OPCUA 实现模式：通过 VitrinSdkNodeManager(CustomNodeManager2) 与 SDK 交互。
/// </summary>
internal sealed class VitrinNodeManagerImpl : IVitrinNodeManager
{
    private readonly ILogger _logger;
    private readonly List<INodeProvider> _providers;

    /// <summary>节点路径索引（path -> NodeState）</summary>
    private readonly Dictionary<string, NodeState> _nodeIndex = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>父节点映射（NodeState -> parent NodeId），用于路径构建</summary>
    private readonly Dictionary<NodeId, NodeId> _parentMap = new();

    /// <summary>Variable 读取处理函数（path -> readFunc）</summary>
    private readonly Dictionary<string, Func<object?>> _readHandlers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Variable 写入处理函数（path -> writeFunc）</summary>
    private readonly Dictionary<string, Action<object?>> _writeHandlers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Method 调用处理函数（path -> onCall）</summary>
    private readonly Dictionary<string, Func<ISystemContext, MethodState, CallMethodRequest, CallMethodResult>> _methodHandlers
        = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>OPC UA NodeId 到路径的映射，用于 Read/Write/Call 回调查找</summary>
    private readonly Dictionary<NodeId, string> _nodeIdToPath = new();

    /// <summary>SDK 节点管理器引用</summary>
    private VitrinSdkNodeManager? _sdkNodeManager;

    /// <summary>命名空间 URI</summary>
    internal const string DefaultNamespaceUri = "http://vitrin.ddon.com/opcua";

    public FolderState ObjectsFolder { get; private set; } = null!;
    public IReadOnlyList<INodeProvider> Providers => _providers.AsReadOnly();

    internal ILogger Logger => _logger;

    public VitrinNodeManagerImpl(IEnumerable<INodeProvider> providers, ILogger logger)
    {
        _providers = providers.Where(p => p != null).ToList();
        _logger = logger;
    }

    internal void SetSdkNodeManager(VitrinSdkNodeManager sdkNodeManager)
    {
        _sdkNodeManager = sdkNodeManager;
    }

    internal ushort NamespaceIndex => _sdkNodeManager?.NamespaceIndex ?? 1;

    /// <summary>
    /// 初始化地址空间，由 VitrinSdkNodeManager.CreateAddressSpace 调用。
    /// </summary>
    internal void Initialize()
    {
        _nodeIndex.Clear();
        _parentMap.Clear();
        _readHandlers.Clear();
        _writeHandlers.Clear();
        _methodHandlers.Clear();
        _nodeIdToPath.Clear();

        try
        {
            ObjectsFolder = _sdkNodeManager!.FindPredefinedNode<FolderState>(
                ObjectIds.ObjectsFolder);
        }
        catch
        {
            _logger.LogWarning("无法获取 ObjectsFolder，使用默认 NodeId。");
            ObjectsFolder = null!;
        }

        foreach (var provider in _providers)
        {
            _logger.LogInformation("正在创建节点提供者 '{RootName}' 的地址空间...", provider.RootName);
            try
            {
                provider.CreateNodes(this);
                _logger.LogInformation("节点提供者 '{RootName}' 地址空间创建完成。", provider.RootName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "节点提供者 '{RootName}' 创建地址空间失败。", provider.RootName);
            }
        }
    }

    // ── 节点查找 ────────────────────────────────────────────────

    public IReadOnlyList<NodeState> GetChildren(string? nodePath = null)
    {
        NodeId? parentNodeId;

        if (string.IsNullOrWhiteSpace(nodePath))
        {
            // 未指定路径 → ObjectsFolder
            if (ObjectsFolder == null) return Array.Empty<NodeState>();
            parentNodeId = ObjectsFolder.NodeId;
        }
        else
        {
            // 指定路径 → 查找节点
            if (!_nodeIndex.TryGetValue(nodePath, out var parentNode))
                return Array.Empty<NodeState>();
            parentNodeId = parentNode.NodeId;
        }

        var result = new List<NodeState>();
        foreach (var parentEntry in _parentMap)
        {
            if (!Equals(parentEntry.Value, parentNodeId)) continue;
            foreach (var entry in _nodeIndex)
            {
                if (Equals(entry.Value.NodeId, parentEntry.Key))
                {
                    result.Add(entry.Value);
                }
            }
        }
        return result.AsReadOnly();
    }

    public NodeState? FindNode(string nodePath)
    {
        if (string.IsNullOrWhiteSpace(nodePath))
            return null;

        if (_nodeIndex.TryGetValue(nodePath, out var node))
            return node;

        return null;
    }

    // ── Folder 节点 ─────────────────────────────────────────────

    public FolderState CreateFolder(BaseObjectState parent, string name)
    {
        var nodeId = new NodeId($"folder_{Guid.NewGuid():N}", NamespaceIndex);
        var folder = new FolderState(null)
        {
            NodeId = nodeId,
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText(name),
            TypeDefinitionId = ObjectTypeIds.FolderType,
        };
        folder.AddReference(ReferenceTypeIds.Organizes, true, parent.NodeId);

        RegisterNode(folder, parent, name);
        return folder;
    }

    // ── Object 节点 ─────────────────────────────────────────────

    public BaseObjectState CreateObject(BaseObjectState parent, string name)
    {
        var nodeId = new NodeId($"obj_{Guid.NewGuid():N}", NamespaceIndex);
        var obj = new BaseObjectState(null)
        {
            NodeId = nodeId,
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText(name),
            TypeDefinitionId = ObjectTypeIds.BaseObjectType,
        };
        obj.AddReference(ReferenceTypeIds.Organizes, true, parent.NodeId);

        RegisterNode(obj, parent, name);
        return obj;
    }

    // ── Variable 节点 ───────────────────────────────────────────

    public BaseVariableState CreateVariable(BaseObjectState parent, string name, Type dataType, Func<object?> readFunc)
    {
        return CreateVariableInternal(parent, name, dataType, readFunc, null);
    }

    public BaseVariableState CreateVariable(BaseObjectState parent, string name, Type dataType,
        Func<object?> readFunc, Action<object?> writeFunc)
    {
        return CreateVariableInternal(parent, name, dataType, readFunc, writeFunc);
    }

    private BaseVariableState CreateVariableInternal(
        BaseObjectState parent, string name, Type dataType,
        Func<object?> readFunc, Action<object?>? writeFunc)
    {
        var uaTypeId = GetDataTypeId(dataType);
        var accessLevel = writeFunc != null
            ? AccessLevels.CurrentReadOrWrite
            : AccessLevels.CurrentRead;

        var nodeId = new NodeId($"var_{Guid.NewGuid():N}", NamespaceIndex);
        var variable = new BaseDataVariableState(null)
        {
            NodeId = nodeId,
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText(name),
            DataType = uaTypeId,
            ValueRank = ValueRanks.Scalar,
            AccessLevel = accessLevel,
            UserAccessLevel = accessLevel,
            MinimumSamplingInterval = 100,
        };
        variable.AddReference(ReferenceTypeIds.Organizes, true, parent.NodeId);

        // 初始值
        try
        {
            variable.Value = readFunc();
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "读取节点 '{Name}' 初始值失败", name);
            variable.StatusCode = StatusCodes.BadWaitingForInitialData;
        }

        var path = BuildNodePath(parent, name);
        _readHandlers[path] = readFunc;
        if (writeFunc != null)
            _writeHandlers[path] = writeFunc;

        RegisterNode(variable, parent, name);
        return variable;
    }

    // ── Method 节点 ─────────────────────────────────────────────

    public MethodState CreateMethod(BaseObjectState parent, string name,
        Func<ISystemContext, MethodState, CallMethodRequest, CallMethodResult> onCall)
    {
        var nodeId = new NodeId($"method_{Guid.NewGuid():N}", NamespaceIndex);
        var method = new MethodState(null)
        {
            NodeId = nodeId,
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText(name),
            Executable = true,
            UserExecutable = true,
        };
        method.AddReference(ReferenceTypeIds.Organizes, true, parent.NodeId);

        var path = BuildNodePath(parent, name);
        _methodHandlers[path] = onCall;

        RegisterNode(method, parent, name);
        return method;
    }

    // ── 节点移除 ────────────────────────────────────────────────

    public bool RemoveNode(string nodePath)
    {
        if (string.IsNullOrWhiteSpace(nodePath) || !_nodeIndex.TryGetValue(nodePath, out var node))
            return false;

        _sdkNodeManager?.RemoveNodeFromAddressSpace(node);

        _nodeIndex.Remove(nodePath);
        _nodeIdToPath.Remove(node.NodeId);
        _parentMap.Remove(node.NodeId);
        _readHandlers.Remove(nodePath);
        _writeHandlers.Remove(nodePath);
        _methodHandlers.Remove(nodePath);

        return true;
    }

    // ── 提交变更 ────────────────────────────────────────────────

    public void ApplyChanges()
    {
        // Gateway 模式下，AddPredefinedNode 已立即注册节点，无需提交。
    }

    // ── SDK 回调入口 ────────────────────────────────────────────

    /// <summary>
    /// 由 VitrinSdkNodeManager.Read 调用。
    /// 对我们管理的节点，通过 readFunc 获取最新值覆盖。
    /// </summary>
    internal void HandleRead(IList<ReadValueId> nodesToRead, IList<DataValue> values, IList<ServiceResult> errors)
    {
        for (int i = 0; i < nodesToRead.Count; i++)
        {
            if (_nodeIdToPath.TryGetValue(nodesToRead[i].NodeId, out var path)
                && _readHandlers.TryGetValue(path, out var readFunc))
            {
                try
                {
                    var val = readFunc();
                    values[i] = new DataValue(new Variant(val), StatusCodes.Good,
                        DateTime.UtcNow, DateTime.UtcNow);
                    errors[i] = ServiceResult.Good;
                }
                catch (Exception ex)
                {
                    _logger.LogTrace(ex, "读取节点 '{Path}' 值时发生异常", path);
                    errors[i] = ServiceResult.Create(StatusCodes.BadWaitingForInitialData, ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// 由 VitrinSdkNodeManager.Write 调用。
    /// 对我们管理的节点，通过 writeFunc 写入实际设备。
    /// </summary>
    internal void HandleWrite(IList<WriteValue> nodesToWrite, IList<ServiceResult> errors)
    {
        for (int i = 0; i < nodesToWrite.Count; i++)
        {
            if (_nodeIdToPath.TryGetValue(nodesToWrite[i].NodeId, out var path)
                && _writeHandlers.TryGetValue(path, out var writeFunc))
            {
                try
                {
                    var value = nodesToWrite[i].Value?.Value;
                    writeFunc(value);
                    errors[i] = ServiceResult.Good;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "写入节点 '{Path}' 值时发生异常", path);
                    errors[i] = ServiceResult.Create(StatusCodes.BadWriteNotSupported, ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// 由 VitrinSdkNodeManager.Call 调用。
    /// 对我们管理的方法节点，执行 onCall 回调。
    /// </summary>
    internal void HandleCall(
        IList<CallMethodRequest> methodsToCall,
        IList<CallMethodResult> results,
        IList<ServiceResult> errors)
    {
        for (int i = 0; i < methodsToCall.Count; i++)
        {
            if (_nodeIdToPath.TryGetValue(methodsToCall[i].MethodId, out var path)
                && _methodHandlers.TryGetValue(path, out var onCall))
            {
                try
                {
                    var methodNode = _sdkNodeManager?.FindPredefinedNode<MethodState>(
                        methodsToCall[i].MethodId);
                    if (methodNode != null)
                    {
                        var result = onCall(
                            _sdkNodeManager!.SystemContext,
                            methodNode,
                            methodsToCall[i]);
                        results[i] = result;
                        errors[i] = ServiceResult.Good;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "调用方法 '{Path}' 时发生异常", path);
                    errors[i] = ServiceResult.Create(StatusCodes.BadInternalError, ex.Message);
                }
            }
        }
    }

    // ── 内部方法 ────────────────────────────────────────────────

    private void RegisterNode(NodeState node, BaseObjectState parent, string name)
    {
        var path = BuildNodePath(parent, name);
        _nodeIndex[path] = node;
        _nodeIdToPath[node.NodeId] = path;

        if (parent != null && parent.NodeId != ObjectIds.ObjectsFolder)
        {
            _parentMap[node.NodeId] = parent.NodeId;
        }

        _sdkNodeManager?.AddNodeToAddressSpace(node);
    }

    private string BuildNodePath(BaseObjectState parent, string name)
    {
        var segments = new List<string> { name };

        // 通过 _parentMap 反向遍历构建路径
        var currentId = parent?.NodeId;
        while (currentId != null && currentId != ObjectIds.ObjectsFolder)
        {
            // 尝试从 _nodeIndex 查找该 NodeId 对应的路径段
            var found = false;
            foreach (var kvp in _nodeIndex)
            {
                if (kvp.Value.NodeId == currentId)
                {
                    var browseName = kvp.Value.BrowseName?.Name;
                    if (!string.IsNullOrEmpty(browseName))
                    {
                        segments.Insert(0, browseName);
                        found = true;
                    }
                    break;
                }
            }

            if (!found)
            {
                // 通过 _parentMap 继续向上查找
            }

            // 继续向父节点遍历
            _parentMap.TryGetValue(currentId, out var nextId);
            currentId = nextId;
        }

        return NodePathBuilder.Build([.. segments]);
    }

    #region 辅助方法

    /// <summary>
    /// CLR Type → OPC UA DataType NodeId（参考 Gateway 的 GetOpcDataType）。
    /// </summary>
    internal static NodeId GetDataTypeId(Type type)
    {
        if (type == typeof(bool)) return DataTypeIds.Boolean;
        if (type == typeof(sbyte)) return DataTypeIds.SByte;
        if (type == typeof(byte)) return DataTypeIds.Byte;
        if (type == typeof(short)) return DataTypeIds.Int16;
        if (type == typeof(ushort)) return DataTypeIds.UInt16;
        if (type == typeof(int)) return DataTypeIds.Int32;
        if (type == typeof(uint)) return DataTypeIds.UInt32;
        if (type == typeof(long)) return DataTypeIds.Int64;
        if (type == typeof(ulong)) return DataTypeIds.UInt64;
        if (type == typeof(float)) return DataTypeIds.Float;
        if (type == typeof(double)) return DataTypeIds.Double;
        if (type == typeof(string)) return DataTypeIds.String;
        if (type == typeof(DateTime)) return DataTypeIds.DateTime;
        return DataTypeIds.BaseDataType;
    }

    #endregion
}

/// <summary>
/// SDK 自定义节点管理器。
/// 参考 Gateway：继承 CustomNodeManager2，管理节点在地址空间中的映射关系。
/// 节点值由 readFunc/writeFunc 处理，而非对象的 OnRead/OnWrite 回调。
/// </summary>
internal sealed class VitrinSdkNodeManager : CustomNodeManager2
{
    private readonly VitrinNodeManagerImpl _impl;
    private readonly ILogger _logger;

    public VitrinSdkNodeManager(IServerInternal server, VitrinNodeManagerImpl impl, ILogger logger)
        : base(server, VitrinNodeManagerImpl.DefaultNamespaceUri)
    {
        _impl = impl;
        _logger = logger;
    }

    /// <summary>
    /// 获取内部实现的 SystemContext（CustomNodeManager2 的 protected 属性）。
    /// </summary>
    internal new ISystemContext SystemContext => base.SystemContext;

    /// <summary>
    /// 获取内部实现的 Lock 对象（CustomNodeManager2 的 protected 属性）。
    /// </summary>
    internal new object Lock => base.Lock;

    /// <summary>
    /// 将节点注册到地址空间（线程安全）。
    /// </summary>
    internal void AddNodeToAddressSpace(NodeState node)
    {
        lock (Lock)
        {
            AddPredefinedNode(SystemContext, node);
        }
    }

    /// <summary>
    /// 从地址空间移除节点（线程安全）。
    /// </summary>
    internal void RemoveNodeFromAddressSpace(NodeState node)
    {
        lock (Lock)
        {
            var references = new List<LocalReference>();
            RemovePredefinedNode(SystemContext, node, references);
        }
    }

    /// <summary>
    /// 创建地址空间时被 SDK 调用。
    /// 参考 Gateway：先调用 base.CreateAddressSpace，再创建自定义节点树。
    /// </summary>
    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        base.CreateAddressSpace(externalReferences);

        _logger.LogInformation("VitrinSdkNodeManager 正在创建地址空间...");
        _impl.Initialize();
        _logger.LogInformation("VitrinSdkNodeManager 地址空间创建完成。");
    }

    /// <summary>
    /// 读取节点值。
    /// 参考 Gateway：先 base.Read，再通过 HandleRead 用 readFunc 获取最新值。
    /// </summary>
    public override void Read(
        OperationContext context,
        double maxAge,
        IList<ReadValueId> nodesToRead,
        IList<DataValue> values,
        IList<ServiceResult> errors)
    {
        base.Read(context, maxAge, nodesToRead, values, errors);
        _impl.HandleRead(nodesToRead, values, errors);
    }

    /// <summary>
    /// 写入节点值。
    /// 参考 Gateway：先 base.Write，再通过 HandleWrite 调用 writeFunc。
    /// </summary>
    public override void Write(
        OperationContext context,
        IList<WriteValue> nodesToWrite,
        IList<ServiceResult> errors)
    {
        base.Write(context, nodesToWrite, errors);
        _impl.HandleWrite(nodesToWrite, errors);
    }

    /// <summary>
    /// 方法调用。
    /// </summary>
    public override void Call(
        OperationContext context,
        IList<CallMethodRequest> methodsToCall,
        IList<CallMethodResult> results,
        IList<ServiceResult> errors)
    {
        base.Call(context, methodsToCall, results, errors);
        // SDK 1.5.374.118 不支持 Call 重写，方法调用通过基类处理
        // 方法处理功能将在后续 SDK 升级后添加
    }
}
