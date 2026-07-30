# OPC UA 管理界面

## 概述

在 VitrinRuntime.Desktop 前端（Vue 3 + Element Plus）中增加 OPC UA 管理页面，提供 Server 启停控制、节点树浏览、节点值读写、后台数据操作等功能。

## 路由与菜单

- 路由路径：`/opcua/server`
- 菜单入口：主页面新增卡片 "OPC UA Server"

## 页面布局

```
┌─────────────────────────────────────────────────────────┐
│  [● 运行中]  OPC UA Server          [重启] [启动/停止]  │  ← 状态栏
│  opc.tcp://localhost:4840                               │
├──────────────────────────┬──────────────────────────────┤
│  地址空间                 │  节点详情                    │
│                          │                              │
│  ─ Objects               │  属性:                       │
│    ├─ PLC                │    名称: D001                │
│    │  ├─ PLC1            │    类型: Int32               │
│    │  │  ├─ D001         │    地址: D100                │
│    │  │  ├─ D002         │    当前值: 150               │
│    │  │  └─ ...          │                              │
│    │  └─ PLC2            │  ┌─────────────────────┐     │
│    ├─ BarcodeScanner     │  │ 新值: [   200   ]   │     │
│    └─ TCPDevice          │  │ [ 写入 ]             │     │
│                          │  └─────────────────────┘     │
│                          │                              │
│                          │  后台操作:                   │
│                          │  [编辑Tag信息] [移除节点]    │
│                          │  [添加Tag]                   │
├──────────────────────────┴──────────────────────────────┤
│  事件日志                                                 │
│  [12:00:01] Server 已启动                                 │
│  [12:00:05] 节点 D001 值变化: 100 → 150                  │
└──────────────────────────────────────────────────────────┘
```

## 页面功能拆分

### 1. Server 状态栏

| 功能 | 说明 |
|------|------|
| 状态指示 | 绿色圆点 + "运行中" / 红色 + "已停止" |
| 端点地址 | 显示 `opc.tcp://localhost:4840` |
| 启动/停止按钮 | 切换 Server 运行状态 |
| 重启按钮 | 先停止再启动 |

### 2. 地址空间树

| 功能 | 说明 |
|------|------|
| 树形浏览 | 递归遍历地址空间，懒加载子节点 |
| 刷新 | 重新遍历，同步运行时新增/删除的节点 |
| 右键菜单 | 复制节点路径、查看属性 |
| 选中节点 | 右侧面板显示详情 |

### 3. 节点详情面板

**展示节点属性：**

| 字段 | 来源 |
|------|------|
| `NodeId` | `BaseObjectState.NodeId` |
| `BrowseName` | `BaseObjectState.BrowseName` |
| `DisplayName` | `BaseObjectState.DisplayName` |
| 数据类型 | `BaseVariableState.DataType` |
| 当前值 | `BaseVariableState.Value` |
| 数据源类型 | PLC Tag / 扫码枪 / 其他 |

**操作：**

| 操作 | 实现方式 |
|------|---------|
| **写入值** | 新值 → Bridge API → 后台决定写 PLC 或直接改 UA Node |
| **编辑Tag信息** | 弹出对话框修改 TagDefinition → 重建 Variable 节点 |
| **移除节点** | 确认 → 后台删除数据源 → NodeProvider 移除节点 |
| **添加Tag** | 弹出对话框输入名称/地址/类型 → 后台注册 Tag → 创建 Variable 节点 |

### 4. 事件日志

| 事件 | 显示格式 |
|------|---------|
| Server 启停 | `[时间] Server 已启动/已停止` |
| 节点值变化 | `[时间] 节点 {path} 值变化: {old} → {new}` |
| 节点新增/移除 | `[时间] 节点 {path} 已添加/已移除` |

## Bridge API 设计

```typescript
// ── src/api/opcUaApi.ts ──

export const opcUaApi = {
  // Server 控制
  getServerStatus(): Promise<ServerStatus>,
  startServer(): Promise<void>,
  stopServer(): Promise<void>,
  restartServer(): Promise<void>,

  // 地址空间浏览
  browseChildren(nodePath: string): Promise<NodeInfo[]>,
  getNodeDetail(nodePath: string): Promise<NodeDetail>,

  // 节点值操作
  readNodeValue(nodePath: string): Promise<any>,
  writeNodeValue(nodePath: string, value: any): Promise<void>,

  // 后台数据操作（非 UA 层面）
  editTagInfo(plcName: string, tagName: string, newInfo: TagEditInfo): Promise<void>,
  removeNode(providerName: string, identifier: string): Promise<void>,
  addTag(plcName: string, tagDef: TagDef): Promise<void>,

  // 事件日志
  getEventLog(since: string): Promise<EventLogEntry[]>,
}
```

## 后端 API 对应

```csharp
[BridgeService(Name = "OpcUaServer")]
public sealed class OpcUaServerService
{
    private readonly IVitrinUaServer _server;
    private readonly IPlcHub _hub;          // 用于后台操作

    [BridgeMethod(Name = "GetServerStatus")]
    public ServerStatusDto GetServerStatus() { ... }

    [BridgeMethod(Name = "BrowseChildren")]
    public List<NodeInfoDto> BrowseChildren(string nodePath) { ... }

    [BridgeMethod(Name = "WriteNodeValue")]
    public async Task WriteNodeValue(WriteNodeValueRequest req)
    {
        // 判断操作方式
        if (req.ThroughPlc)
        {
            // 方案 A：走 PLC 写入（推荐，保持一致性）
            var session = _hub.For(req.PlcName);
            await session.SetAsync(req.TagName, req.Value);
        }
        else
        {
            // 方案 B：直接修改 UA Node 值（仅内存，用于模拟）
            var node = _server.NodeManager.FindNode(req.NodePath);
            (node as BaseVariableState)!.Value = req.Value;
            _server.NodeManager.ApplyChanges();
        }
    }
}
```

## 状态类定义

```csharp
public sealed class ServerStatusDto
{
    public bool IsRunning { get; set; }
    public string EndpointUrl { get; set; } = "";
    public string ServerName { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public int SessionCount { get; set; }
}

public sealed class NodeInfoDto
{
    public string NodePath { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string NodeClass { get; set; } = "";    // "Object" / "Variable" / "Method"
    public string DataType { get; set; } = "";
    public bool HasChildren { get; set; }
}

public sealed class NodeDetailDto
{
    public string NodePath { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string NodeClass { get; set; } = "";
    public string DataType { get; set; } = "";
    public string? Value { get; set; }
    public string? SourceType { get; set; }  // "PLC_READONLY" / "PLC_READWRITE" / "SIMULATION"
    public string? PlcName { get; set; }
    public string? TagName { get; set; }
}
```
