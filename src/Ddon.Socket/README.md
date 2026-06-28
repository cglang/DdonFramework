# Ddon.Socket

基于 `Ddon.Pipeline` 中间件管道的 TCP Socket 通信框架，支持 Server 和 Client 两种模式。

## 架构

```
ISocketManager
  ├─ UseServer("Srv")  → TcpListener → Accept → 动态 Endpoint[]
  └─ AddEndpoint("Cli") → TcpClient → Connect → 单个 Endpoint

每个 Endpoint: Worker → Protocol → Pipeline → Dispatcher → Handler[]
```

## 快速开始

### Server 模式
```csharp
services.AddSocket(builder =>
{
    builder.UseServer("MyServer", opt =>
    {
        opt.Port = 8888;
    }, endpoint =>
    {
        endpoint.UsePipeline(p => p.Use<LogMiddleware>());
        endpoint.AddHandler<MyHandler>();
    });
});
services.AddSocketHostedService();
```

### Client 模式
```csharp
services.AddSocket(builder =>
{
    builder.AddEndpoint("Remote", endpoint =>
    {
        endpoint.Configure(o =>
        {
            o.Host = "192.168.1.100";
            o.Port = 8888;
        });
        endpoint.UseProtocol<LengthPrefixProtocol>();
        endpoint.UsePipeline(p => p.Use<LogMiddleware>());
        endpoint.AddHandler<MyHandler>();
    });
});
services.AddSocketHostedService();
```

### 普通模式（无 DI）
```csharp
var manager = new SocketManager();
manager.UseServer("Srv", o => o.Port = 8888, endpoint =>
{
    endpoint.AddHandler<MyHandler>();
});
manager.AddEndpoint("Cli", endpoint =>
{
    endpoint.Configure(o => { o.Host = "10.0.0.1"; o.Port = 8888; });
});
await manager.StartAllAsync();
```

## 项目结构
```
Abstractions/      9 接口定义
Builder/           3 Builder 类
Configuration/     SocketClientOptions, SocketServerOptions
Core/              7 核心实现（含 SocketServer）
Hosted/            1 BackgroundService
Models/            SocketContext
Protocols/         LengthPrefixProtocol, LineProtocol
Extensions/        DI 注册扩展
```

## 核心概念
- **Endpoint** — 一个 TCP 连接的完整抽象（Worker + Pipeline + Dispatcher + Handlers）
- **Worker** — 基于 `TcpClient`/`NetworkStream` 的异步 IO
- **Pipeline** — 中间件链，复用 Ddon.Pipeline
- **Server** — `TcpListener` 监听 + accept，每个连接自动创建 Endpoint
- **Protocol** — 编解码（内置 `LengthPrefixProtocol`、`LineProtocol`）

## 数据流
```
Client: TcpClient → Connect → Worker.ReceiveLoop → Protocol.Decode → Pipeline → Handler
Server: TcpListener → Accept → Worker(accepted) → Protocol.Decode → Pipeline → Handler
```

## 依赖
- Ddon.Pipeline
