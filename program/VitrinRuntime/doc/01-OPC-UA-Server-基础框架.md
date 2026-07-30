# OPC UA Server 基础框架

## 概述

构建 OPC UA Server 核心骨架，提供启动/停止控制，并定义 `INodeProvider` 接口作为所有设备模块接入 OPC UA 地址空间的统一规范。

## 项目结构

```
src/
├── Ddon.OpcUaServer/           # 新建项目（net8.0）
│       ├── Server/
│       │   ├── IVitrinUaServer.cs       # Server 接口
│       │   ├── VitrinUaServer.cs         # Server 实现
│       │   └── VitrinUaServerOptions.cs  # Server 配置
│       ├── NodeManager/
│       │   ├── INodeProvider.cs          # 节点提供者接口
│       │   └── VitrinNodeManager.cs      # 节点管理器
│       └── Nodes/
│           └── NodePathBuilder.cs        # 节点路径工具
```

## 核心接口

### IVitrinUaServer

```csharp
public interface IVitrinUaServer : IAsyncDisposable
{
    /// <summary>Server 当前是否正在运行。</summary>
    bool IsRunning { get; }

    /// <summary>Server 绑定的端点地址。</summary>
    string EndpointUrl { get; }

    /// <summary>节点管理器（持有地址空间所有节点）。</summary>
    IVitrinNodeManager NodeManager { get; }

    /// <summary>启动 OPC UA Server。</summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>停止 OPC UA Server。</summary>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>Server 状态变化事件。</summary>
    event EventHandler<ServerStatusChangedEventArgs>? StatusChanged;
}
```

### INodeProvider

```csharp
public interface INodeProvider
{
    /// <summary>提供者在 ObjectsFolder 下的根节点名称（如 "PLC"、"BarcodeScanner"）。</summary>
    string RootName { get; }

    /// <summary>
    /// Server 启动/初始化时调用。提供者在此创建自己的节点子树。
    /// </summary>
    void CreateNodes(INodeManager nodeManager);

    /// <summary>
    /// 运行时动态添加子节点（新增设备实例）。
    /// </summary>
    void AddChildNode(INodeManager nodeManager, string identifier);

    /// <summary>
    /// 运行时移除子节点。
    /// </summary>
    void RemoveChildNode(string identifier);
}
```

### INodeManager

```csharp
public interface INodeManager
{
    /// <summary>根据路径字符串查找节点。</summary>
    BaseObjectState? FindNode(string nodePath);

    /// <summary>在指定父节点下创建 Folder 节点。</summary>
    FolderState CreateFolder(BaseObjectState parent, string name);

    /// <summary>创建 Variable 节点（只读，从内存镜像读取）。</summary>
    BaseVariableState CreateVariable(BaseObjectState parent, string name, Type dataType, Func<object?> readFunc);

    /// <summary>创建 Variable 节点（可读写，写时调用 writeFunc）。</summary>
    BaseVariableState CreateVariable(BaseObjectState parent, string name, Type dataType,
        Func<object?> readFunc, Action<object?> writeFunc);

    /// <summary>创建 Method 节点。</summary>
    MethodState CreateMethod(BaseObjectState parent, string name, Func<ISystemContext, MethodState, CallMethodRequest, CallMethodResult> onCall);

    /// <summary>从地址空间移除节点。</summary>
    bool RemoveNode(string nodePath);

    /// <summary>提交地址空间变更，触发 SDK 更新订阅者的 MonitoredItem。</summary>
    void ApplyChanges();

    /// <summary>获取所有已注册的 INodeProvider。</summary>
    IReadOnlyList<INodeProvider> Providers { get; }
}
```

## VitrinUaServer 启动流程

```
DI 容器
  ├── services.AddSingleton<IVitrinUaServer, VitrinUaServer>()
  └── services.AddSingleton<INodeProvider, PlcNodeProvider>()  ← 各模块自注册

Server 启动:
  1. 创建 ApplicationInstance（处理证书）
  2. 加载 ServerConfiguration
  3. 创建 VitrinNodeManager 实例
  4. 遍历 DI 收集的 INodeProvider 列表
  5. 依次调用 provider.CreateNodes(nodeManager)
  6. 将节点树挂载到 ObjectsFolder 下
  7. 绑定 Endpoint 并启动 Server
  8. 触发 StatusChanged 事件

Server 停止:
  1. 停止 Server 内核
  2. 清理节点树
  3. 触发 StatusChanged 事件
```

## Server 配置（VitrinUaServerOptions）

```csharp
public class VitrinUaServerOptions
{
    /// <summary>OPC UA Server 端点地址，默认 "opc.tcp://localhost:4840"</summary>
    public string EndpointUrl { get; set; } = "opc.tcp://localhost:4840";

    /// <summary>Server 名称，显示在 UA Client 的发现列表中。</summary>
    public string ServerName { get; set; } = "VitrinRuntime";

    /// <summary>证书存储目录，默认在 AppData 下。</summary>
    public string CertificateStorePath { get; set; } = "";

    /// <summary>是否允许外部客户端匿名连接。</summary>
    public bool AllowAnonymous { get; set; } = true;

    /// <summary>最大会话数。</summary>
    public uint MaxSessionCount { get; set; } = 100;
}
```

## DI 注册扩展

```csharp
public static class ServiceCollectionUaServerExtensions
{
    public static IServiceCollection AddVitrinUaServer(
        this IServiceCollection services,
        Action<VitrinUaServerOptions>? configure = null)
    {
        // 注册配置
        services.AddSingleton(provider =>
        {
            var opts = new VitrinUaServerOptions();
            configure?.Invoke(opts);
            return opts;
        });

        // 注册 Server 和节点管理器
        services.AddSingleton<IVitrinNodeManager, VitrinNodeManager>();
        services.AddSingleton<IVitrinUaServer, VitrinUaServer>();

        // 允许后续模块自动注册 INodeProvider
        services.AddSingleton<INodeProvider>(sp =>
        {
            // 这里会被各模块的 AddSingleton<INodeProvider, XxxProvider>() 替代
            // 只是一个占位，确保 IEnumerable<INodeProvider> 不会为空
            return null!; // 实际会被覆写
        });

        return services;
    }
}
```

> 建议：在 VitrinRuntime.Desktop 的 App.axaml.cs 中调用 `services.AddVitrinUaServer()`，各模块按需添加 `INodeProvider` 实现。

## NuGet 依赖

目标框架 **net8.0**（Opc.Ua 最低要求 net6.0）：

```xml
<PackageReference Include="OPCFoundation.NetStandard.Opc.Ua" Version="1.5.374.96" />
<PackageReference Include="OPCFoundation.NetStandard.Opc.Ua.Core" Version="1.5.374.96" />
<PackageReference Include="OPCFoundation.NetStandard.Opc.Ua.Server" Version="1.5.374.96" />
```

## 证书处理

OPC UA 强制要求 Server 证书。首次启动时自动处理：

```csharp
private void EnsureCertificate()
{
    var app = new ApplicationInstance
    {
        ApplicationName = _options.ServerName,
        ApplicationType = ApplicationType.Server
    };

    // 如果证书不存在则自动创建自签名证书
    var certificate = app.GetApplicationCertificate(
        new CertificateFactory(), createIfNotExists: true);

    _serverConfiguration.SecurityConfiguration.ApplicationCertificate =
        certificate?.RawData.ToArray();
}
```

Server 的证书被 Client 信任后即可建立安全连接。
