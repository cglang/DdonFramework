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

## 项目结构 (22 源文件)
```
Abstractions/      9 接口
Builder/           3 Builder 类
Configuration/     SocketClientOptions, SocketServerOptions
Core/              7 实现（SocketServer 为新增）
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

## 内置 Protocols
- **LengthPrefixProtocol** — 4 字节长度前缀 + payload
- **LineProtocol** — 按 `\n` 分割行文本

## 目标框架
netstandard2.0;net8.0
