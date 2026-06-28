# Ddon.OpenProtocol

基于 Ddon.Socket + Ddon.Pipeline 的 Open Protocol TCP Client 框架。

## 架构

```
Application
    │
    ▼
IOpenProtocolManager
    └─ IOpenProtocolEndpoint
          ├─ ISocketWorker (Ddon.Socket)  ← TCP 传输
          ├─ IOpenProtocolProtocol        ← MID 编解码（NUL 可配置）
          ├─ RequestResponseMatcher       ← 请求响应匹配 + Mid0004 错误处理
          ├─ OpenProtocolEventBus         ← 推送事件分发（按 MID 号）
          ├─ IOpenProtocolPipeline        ← 中间件链（可选）
          └─ IReconnectStrategy           ← 断线重连
```

## 快速开始

### 1. DI 注册

```csharp
services.AddOpenProtocol(builder =>
{
    builder.AddEndpoint("Tightener", endpoint =>
    {
        endpoint.Configure(o =>
        {
            o.Host = "192.168.1.100";
            o.Port = 4545;
        });

        endpoint.UsePipeline(p => p.Use<LoggingMiddleware>());
        endpoint.AddHandler<TighteningHandler>();
    });
});

services.AddOpenProtocolHostedService();
```

### 2. 手动创建

```csharp
var socketFactory = new SocketFactory();
var manager = new OpenProtocolManager(socketFactory);

manager.AddEndpoint("Tightener", endpoint =>
{
    endpoint.Configure(o =>
    {
        o.Host = "192.168.1.100";
        o.Port = 4545;
    });
});

await manager.StartAllAsync();
var ep = manager.GetEndpoint("Tightener");
await ep.SendAsync<Mid0002>(new Mid0001());
```

## 核心 API

### 请求/响应

```csharp
// 发请求并等待对应响应
var result = await endpoint.SendAsync<Mid0002>(new Mid0001());
Console.WriteLine($"握手完成, MID0002 received");
```

### 订阅 + 等待首个结果

```csharp
// 订阅拧紧结果 + 等待首个响应
var first = await endpoint.SubscribeAsync<Mid0061>(new Mid0060());
Console.WriteLine($"首拧: Torque={first.Torque}Nm");

// 后续结果通过事件订阅接收
endpoint.Subscribe<Mid0061>(result =>
{
    Console.WriteLine($"拧紧完成: {result.TighteningStatus}");
});
```

### 持久订阅（断线重连后自动重发）

```csharp
await endpoint.RegisterSubscriptionAsync(new Mid0060());
endpoint.Subscribe<Mid0061>(OnTighteningResult);
```

### Push 事件

```csharp
// 订阅指定 MID
endpoint.Subscribe<Mid0061>(OnTighteningResult);
endpoint.Subscribe<Mid0072>(OnAlarm);

// 订阅所有 MID（调试用）
endpoint.SubscribeAll(mid =>
{
    Console.WriteLine($"← MID{mid.Header.Mid:D4}");
    return Task.CompletedTask;
});

// 订阅返回 IDisposable，取消时释放
IDisposable sub = endpoint.Subscribe<Mid0061>(handler);
sub.Dispose(); // 取消订阅
```

## 配置

### 终止符配置

解决控制器对消息结尾的不同要求：

```csharp
endpoint.Configure(o =>
{
    // None  - 无终止符
    // Nul   - \0 (默认)
    // CrLf  - \r\n
    // Custom - 自定义字节
    o.Terminator = MessageTerminator.None;
    o.CustomTerminator = new byte[] { 0x00 };
});
```

### Mid0004 错误处理

Open Protocol 控制器可能返回 Mid0004 表示命令失败：

```csharp
try
{
    await endpoint.SendAsync<Mid0005>(new Mid0018());
}
catch (OpenProtocolException ex) when (ex.FailedMid == 0018)
{
    Console.WriteLine($"命令 Mid0018 被拒绝, 错误码: {ex.ErrorCode}");
}
```

### 自定义 MID

```csharp
public class MyMid0999 : Mid
{
    public const int MID = 999;

    public MyMid0999() : base(new Header(MID))
    {
    }

    public string CustomField { get; set; } = string.Empty;
}

// 注册到端点（自动读取 MID 常量）
endpoint.RegisterCustomMid<MyMid0999>();
endpoint.MapResponse<MyMid0999, Mid0005>();
```

## 项目结构

```
Ddon.OpenProtocol/
├── Abstractions/      6 接口
│   ├── IOpenProtocolManager
│   ├── IOpenProtocolEndpoint
│   ├── IOpenProtocolProtocol
│   ├── IOpenProtocolHandler
│   ├── IOpenProtocolMiddleware
│   └── IOpenProtocolPipeline
├── Builder/           2 Builder
│   ├── OpenProtocolBuilder
│   └── OpenProtocolEndpointBuilder
├── Configuration/     2 Options
│   ├── OpenProtocolOptions
│   └── OpenProtocolEndpointOptions
├── Core/              8 实现
│   ├── OpenProtocolManager
│   ├── OpenProtocolEndpoint
│   ├── OpenProtocolProtocol
│   ├── RequestResponseMatcher
│   ├── OpenProtocolEventBus
│   ├── OpenProtocolPipeline
│   ├── OpenProtocolDispatcher
│   └── PacketFramer
├── Hosted/            1 HostedService
│   └── OpenProtocolHostedService
├── Models/            2 Model
│   ├── OpenProtocolContext
│   └── OpenProtocolException
└── Extensions/        1 DI 注册
    └── ServiceCollectionExtensions
```

## 数据流

```
Send:
  SendAsync<Mid0002>(new Mid0001())
    → _matcher.Enqueue(requestMid=1, timeout)
    → _protocol.Serialize(mid)       [按 Terminator 配置追加 NUL/CrLf/...]
    → _worker.SendAsync(bytes)

Recv:
  _worker.DataReceived
    → ReceiveLoopAsync (Channel 后台循环)
    → PacketFramer 拆帧
    → _protocol.Deserialize(packet)   [含 SafeParse revision 降级]
    → _matcher.TryComplete(mid)
        ├─ matched (含 Mid0004) → 完成 Task (或抛 OpenProtocolException)
        └─ unmatched → Pipeline → EventBus → Dispatcher
```

## 依赖

- Ddon.Socket — TCP 传输层（ISocketWorker）
- Ddon.Pipeline — 中间件管道
- OpenProtocolInterpreter (NuGet) — MID 协议解析

## 与旧版的区别

| 旧版问题 | 新版解决 |
|----------|----------|
| NUL 终止符写死 | `MessageTerminator` 枚举可配置 |
| Mid0004 无法处理 | `RequestResponseMatcher` 自动路由 + `OpenProtocolException` |
| 订阅结果无法等待 | `SubscribeAsync<T>` 返回首个响应 |
| 命名空间/结构混乱 | 七层 Ddon.Socket 同构结构 + `Ddon.OpenProtocol.*` 统一命名空间 |
| 自定义 MID 不便捷 | `RegisterCustomMid<T>()` 类型驱动，无需硬编码 MID 号 |
