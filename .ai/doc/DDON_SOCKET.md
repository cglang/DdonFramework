# Ddon.Socket TCP Socket 通信框架

## 用途
基于 `Ddon.Pipeline` 的 TCP Socket 框架，支持 Server 和 Client 两种模式，与 Ddon.Serial 同构。

## 架构
```
ISocketManager
  ├─ UseServer("Srv") → TcpListener → Accept → 动态 Endpoint[]
  └─ AddEndpoint("Cli") → TcpClient → Connect → 单个 Endpoint
```
每个 Endpoint: Worker → Protocol → Pipeline → Dispatcher → Handler[]

## 项目结构 (23 源文件)
```
Abstractions/      9 接口
Builder/           3 Builder 类
Configuration/     SocketClientOptions, SocketServerOptions
Core/              8 实现（SocketServer + DefaultReconnectStrategy）
Hosted/            1 BackgroundService
Models/            SocketContext
Protocols/         LengthPrefixProtocol, LineProtocol
Extensions/        DI 注册扩展
```

## 与 Serial 架构对照
| 概念 | Serial | Socket |
|------|--------|--------|
| Worker | SerialPort.BaseStream | TcpClient/NetworkStream |
| Context | PortName/Buffer | ConnectionId/RemoteEndPoint/Buffer |
| Server | 不支持 | TcpListener + accept 循环 |
| Client | COM 口打开 | TcpClient.ConnectAsync |

## 数据流
```
Client: TcpClient → Connect → Worker.ReceiveLoop → Protocol.Decode → Pipeline → Handler
Server: TcpListener → Accept → Worker(accepted) → Protocol.Decode → Pipeline → Handler
```

## 使用示例
```csharp
// Server
builder.UseServer("MyServer", o => o.Port = 8888, endpoint =>
{
    endpoint.UsePipeline(p => p.Use<LogMiddleware>());
    endpoint.AddHandler<MyHandler>();
});

// Client
builder.AddEndpoint("Remote", endpoint =>
{
    endpoint.Configure(o => { o.Host = "10.0.0.1"; o.Port = 8888; });
    endpoint.UseProtocol<LengthPrefixProtocol>();
    endpoint.AddHandler<MyHandler>();
});
```

## 断线自动重连 (Client 模式)

Client 模式支持自动重连。当远程断开连接时，`SocketEndpoint` 自动启动重连循环。

- **配置**: `endpoint.UseReconnect<DefaultReconnectStrategy>()`
- **DefaultReconnectStrategy**: 指数退避策略 1s → 2s → 4s → 8s → 15s → 封顶 30s
- **无策略**: 若未调用 `UseReconnect`，断开后不会重连，标记 `IsRunning = false`
- **Server 模式**: 接受连接的 Endpoint 不会自动重连

### 重连流程
```
OnDisconnected → ConnectWithRetryAsync(reconnectStrategy)
                   ├─ 成功 → IsRunning = true
                   └─ 失败 → delay → 继续重试
                  StopAsync → _cts.Cancel() → 终止重连
```

```csharp
builder.AddEndpoint("Remote", endpoint =>
{
    endpoint.Configure(o => { o.Host = "10.0.0.1"; o.Port = 8888; });
    endpoint.UseReconnect<DefaultReconnectStrategy>();
    endpoint.AddHandler<MyHandler>();
});
```

## 内置 Protocols
- **LengthPrefixProtocol** — 4 字节长度前缀 + payload
- **LineProtocol** — 按 `\n` 分割行文本

## 目标框架
netstandard2.0;net8.0
