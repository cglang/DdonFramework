# PlcNodeProvider 实现

## 概述

实现 `INodeProvider` 接口，将 `IPlcHub` 中管理的 PLC 和 Tag 映射为 OPC UA 地址空间中的 Object/Variable 节点。

## 地址空间结构

```
ObjectsFolder
└── PLC (Folder)
    ├── PLC1 (Object)
    │   ├── D001 (Variable, Int32)    ← TagDefinition
    │   ├── D002 (Variable, Bool)
    │   ├── D003 (Variable, Float)
    │   └── ...
    ├── PLC2 (Object)
    │   └── ...
    └── ...
```

## 节点映射规则

| OPC UA 节点 | 对应实体 | 属性 |
|------------|---------|------|
| `PLC` Folder | 所有 PLC 的容器 | BrowseName = "PLC" |
| `PLC1` Object | 单个 PLC 实例 | BrowseName = plcName |
| `D001` Variable | 单个 Tag | BrowseName = tagName; DataType = 映射到 OPC UA 类型 |

## 数据类型映射

```csharp
private static readonly Dictionary<PlcDataType, Type> PlcToUaType = new()
{
    [PlcDataType.Bool]   = typeof(bool),
    [PlcDataType.Byte]   = typeof(byte),
    [PlcDataType.Int16]  = typeof(short),
    [PlcDataType.UInt16] = typeof(ushort),
    [PlcDataType.Int32]  = typeof(int),
    [PlcDataType.UInt32] = typeof(uint),
    [PlcDataType.Float]  = typeof(float),
    [PlcDataType.Double] = typeof(double),
    [PlcDataType.String] = typeof(string),
};
```

## 核心实现

```csharp
public sealed class PlcNodeProvider : INodeProvider
{
    private readonly IPlcHub _hub;
    private readonly ILogger<PlcNodeProvider> _logger;

    // PLC名称 → PLC Object 节点
    private readonly Dictionary<string, BaseObjectState> _plcNodes = new();
    // "PLC名称.Tag名称" → Variable 节点
    private readonly Dictionary<string, BaseVariableState> _tagNodes = new();

    public string RootName => "PLC";

    public PlcNodeProvider(IPlcHub hub, ILogger<PlcNodeProvider> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    /// <summary>Server 启动/初始化时调用，遍历现有 PLC 创建节点树。</summary>
    public void CreateNodes(INodeManager nodeManager)
    {
        // 1. 在 ObjectsFolder 下创建 "PLC" Folder
        var plcRoot = nodeManager.CreateFolder(nodeManager.ObjectsFolder, "PLC");

        // 2. 遍历 IPlcHub 中所有已注册的 PLC
        foreach (var plcName in _hub.Names)
        {
            CreatePlcNode(nodeManager, plcRoot, plcName);
        }
    }

    /// <summary>运行时添加 PLC（在 IPlcHub.AddPlcAsync 之后调用）。</summary>
    public void AddChildNode(INodeManager nodeManager, string plcName)
    {
        var plcRoot = nodeManager.FindNode("PLC")
            ?? nodeManager.CreateFolder(nodeManager.ObjectsFolder, "PLC");

        // 如果已存在则不重复创建
        if (_plcNodes.ContainsKey(plcName)) return;

        CreatePlcNode(nodeManager, plcRoot, plcName);
        nodeManager.ApplyChanges();
    }

    /// <summary>运行时移除 PLC。</summary>
    public void RemoveChildNode(string plcName)
    {
        if (_plcNodes.Remove(plcName, out var plcNode))
        {
            // 移除该 PLC 下所有 Tag 节点记录
            var keysToRemove = _tagNodes.Keys
                .Where(k => k.StartsWith(plcName + "."))
                .ToList();
            foreach (var key in keysToRemove)
                _tagNodes.Remove(key);

            _logger.LogInformation("已从地址空间移除 PLC '{Plc}'", plcName);
        }
    }

    private void CreatePlcNode(INodeManager nodeManager, BaseObjectState parent, string plcName)
    {
        var session = _hub.For(plcName);
        var plcNode = nodeManager.CreateObject(parent, plcName);
        _plcNodes[plcName] = plcNode;

        foreach (var tag in session.Tags)
        {
            CreateTagVariable(nodeManager, plcNode, plcName, tag);
        }

        _logger.LogInformation("PLC '{Plc}' 节点树已创建（{Count} 个 Tag）",
            plcName, session.Tags.Count);
    }

    private void CreateTagVariable(
        INodeManager nodeManager,
        BaseObjectState parent,
        string plcName,
        TagDefinition tag)
    {
        var uaType = PlcToUaType.GetValueOrDefault(tag.Type, typeof(object));

        var session = _hub.For(plcName);
        var variable = nodeManager.CreateVariable(
            parent,
            tag.Name,
            uaType,
            readFunc: () =>
            {
                // 从内存镜像读取当前值（零开销）
                try { return session.Get<object>(tag.Name); }
                catch { return null; }
            },
            writeFunc: (value) =>
            {
                // 写入 PLC（异步转同步，通过 Task.Run 避免阻塞）
                Task.Run(async () =>
                {
                    try { await session.SetAsync(tag.Name, value); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "写入 Tag '{Tag}' 失败", tag.Name);
                    }
                });
            }
        );

        // 保存 Tag 名称到节点字典，用于后续查找
        _tagNodes[plcName + "." + tag.Name] = variable;
    }

    /// <summary>运行时添加 Tag（在 IPlcSession.AddTag 之后调用）。</summary>
    public void AddTag(INodeManager nodeManager, string plcName, TagDefinition tag)
    {
        if (!_plcNodes.TryGetValue(plcName, out var plcNode))
            return;

        CreateTagVariable(nodeManager, plcNode, plcName, tag);
        nodeManager.ApplyChanges();
    }

    /// <summary>运行时移除 Tag。</summary>
    public void RemoveTag(string plcName, string tagName)
    {
        var key = plcName + "." + tagName;
        if (_tagNodes.TryGetValue(key, out var variable))
        {
            nodeManager.RemoveNode(variable.NodeId.ToString());
            _tagNodes.Remove(key);
        }
    }
}
```

## 运行时同步机制

### 场景 1：运行时添加 PLC

```csharp
// ── 用户通过管理界面添加 PLC ──

public async Task AddPlcWithUaNodeAsync(string name, IPlcClient client, ...)
{
    // 1. 添加到现有系统
    await _hub.AddPlcAsync(name, client, options);

    // 2. 通知 OPC UA Server 更新地址空间
    var provider = _server.NodeManager.Providers
        .OfType<PlcNodeProvider>().FirstOrDefault();

    if (provider is not null)
    {
        provider.AddChildNode(_server.NodeManager, name);
    }
}
```

### 场景 2：运行时添加/移除 Tag

```csharp
// ── 用户通过管理界面添加 Tag ──

public async Task AddTagWithUaNodeAsync(string plcName, TagDefinition tag)
{
    var session = _hub.For(plcName);
    session.AddTag(tag);

    var provider = _server.NodeManager.Providers
        .OfType<PlcNodeProvider>().FirstOrDefault();

    if (provider is not null)
    {
        provider.AddTag(_server.NodeManager, plcName, tag);
    }
}
```

### 场景 3：动态 Tag（不预定义，运行时从 PLC 发现）

如果后续需要支持 Tag 自动发现，可以在 `PlcNodeProvider` 中添加定时扫描：

```csharp
// 伪代码：定时从 PLC 读取 Tag 列表
// 对比现有节点树，新增/移除差异节点
// 通过 GeneralEventBus 或定时器触发

GeneralEventBus.Default.Subscribe((PlcTagDiscoveredEvent evt) =>
{
    AddTag(nodeManager, evt.PlcName, evt.Tag);
});
```

## IM 方式与 OPC UA 方式的对比

| 对比项 | 读 Tag 值 | 写 Tag 值 |
|--------|----------|----------|
| 现有方式 | `session.Get<T>()` 读镜像 | `session.SetAsync<T>()` 写 PLC |
| OPC UA 方式 | UA Client Read 请求 → readFunc | UA Client Write 请求 → writeFunc |
| 进程内操作 | `variable.Value` 直接赋值 | 同左（只改内存，不写 PLC） |

两者通过 `readFunc` / `writeFunc` 桥接，**底层数据链路完全复用现有逻辑**。
