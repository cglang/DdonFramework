# Ddon.Serial

串口通信框架，基于 `Ddon.Pipeline` 实现中间件管道，支持多 COM 口独立运行。

## 快速开始

```csharp
// DI 模式
services.AddSerial(builder =>
{
    builder.AddEndpoint("PLC", endpoint =>
    {
        endpoint.Configure(o =>
        {
            o.PortName = "COM1";
            o.BaudRate = 115200;
        });

        endpoint.UseProtocol<ModbusProtocol>();
        endpoint.UseReconnect<DefaultReconnectStrategy>();

        endpoint.UsePipeline(pipeline =>
        {
            pipeline.Use<LoggingMiddleware>();
            pipeline.Use<CrcMiddleware>();
        });

        endpoint.AddHandler<LoggerHandler>();
        endpoint.AddHandler<PlcHandler>();
    });
});

services.AddSerialHostedService();
```

```csharp
// 普通模式
var manager = new SerialManager();
manager.AddEndpoint("COM3", endpoint =>
{
    endpoint.Configure(o => o.PortName = "COM3");
    endpoint.AddHandler<MyHandler>();
});
await manager.StartAllAsync();
```

## 项目结构
```
Abstractions/      9 接口定义
Builder/           3 Builder 类
Configuration/     2 Options 类
Core/              6 核心实现
Hosted/            1 BackgroundService
Models/            2 数据模型
Extensions/        DI 注册扩展
```

## 核心概念
- **Endpoint** — 一个 COM 口的完整抽象（Worker + Pipeline + Dispatcher + Handlers）
- **Worker** — 串口 IO，基于 `BaseStream.ReadAsync` 异步实现
- **Pipeline** — 中间件链，复用 Ddon.Pipeline
- **Dispatcher** — 将处理后数据分发给所有 Handler
- **Protocol** — 编解码与帧解析
- **ReconnectStrategy** — 断线重连策略

## 数据流
```
COM → Worker.ReadAsync() → Protocol.Decode() → Pipeline → Dispatcher → Handler[]
```

## 依赖
- Ddon.Pipeline
- System.IO.Ports
