# Ddon.Serial 串口通信框架

## 设计目标
- 支持多个 COM 口同时工作，每个 COM 独立配置、独立生命周期
- 每个 COM 拥有独立 Pipeline、Handler、Protocol、重连策略
- 支持 Microsoft Dependency Injection 和 Generic Host
- 支持可选 HostedService 自动管理

## 总体架构
```
Application → ISerialManager
                  ├─ Endpoint(COM1) → Worker → Pipeline → Dispatcher → Handler[]
                  ├─ Endpoint(COM2) → Worker → Pipeline → Dispatcher → Handler[]
                  └─ Endpoint(COM3) → Worker → Pipeline → Dispatcher → Handler[]
```
Manager 不参与数据处理，仅负责 Endpoint 的创建、启动、停止和管理。

## 项目结构 (24 文件)
```
Ddon.Serial/
├─ Abstractions/         (9 接口)
│  ├─ ISerialManager      创建/删除/获取 Endpoint，StartAll/StopAll
│  ├─ ISerialEndpoint     单个 COM 端点（Start/Stop）
│  ├─ ISerialWorker       串口 IO（Open/Close/Read/Write），事件驱动
│  ├─ ISerialHandler      业务处理器 HandleAsync(SerialContext)
│  ├─ ISerialPipeline     继承 IGeneralCustomPipeline<SerialContext>
│  ├─ ISerialMiddleware   继承 IGeneralPipelineMiddleware<SerialContext>
│  ├─ ISerialProtocol     编码 Encode / 解码 Decode（帧解析）
│  ├─ IReconnectStrategy  重连延迟策略 GetNextDelay/Reset
│  └─ ISerialFactory      Worker/Protocol/Strategy 创建工厂
│
├─ Builder/              (3 类)
│  ├─ SerialBuilder         顶级入口：AddEndpoint(name, configure)
│  ├─ SerialEndpointBuilder 配置单个端点：Configure/UseProtocol/UsePipeline/AddHandler
│  └─ PipelineBuilder       配置管道中间件链
│
├─ Configuration/
│  ├─ SerialOptions         全局选项
│  └─ SerialPortOptions     串口参数（PortName/BaudRate/Parity/DataBits/StopBits...）
│
├─ Core/                 (6 实现)
│  ├─ SerialManager         ConcurrentDictionary<string, ISerialEndpoint>
│  ├─ SerialEndpoint        Worker+Protocol+Pipeline+Dispatcher 编排核心
│  ├─ SerialWorker          基于 SerialPort.BaseStream.ReadAsync 的异步实现
│  ├─ SerialDispatcher      遍历 Handler 列表逐个分发
│  ├─ SerialPipeline        Ddon.Pipeline 适配包装
│  └─ SerialFactory         DI 优先、Activator 回退
│
├─ Hosted/
│  └─ SerialHostedService   BackgroundService → StartAll / StopAll
│
├─ Models/
│  ├─ SerialMessage         PortName/Buffer/Length/ReceiveTime/Metadata
│  └─ SerialContext         管道上下文（含 ParsedMessage）
│
└─ Extensions/
   └─ ServiceCollectionExtensions  AddSerial() + AddSerialHostedService()
```

## 数据接收流程
```
COM3 → Read() → Protocol.Decode() → Pipeline → Dispatcher → Handler[]
```
- 协议解码支持部分帧缓冲，不完整帧保留在 Endpoint 的内部 `_receiveBuffer`
- 管道是用户可自定义的中间件链
- 分派器调用所有注册的 Handler

## 配置示例

### DI 模式
```csharp
services.AddSerial(builder =>
{
    builder.AddEndpoint("PLC", endpoint =>
    {
        endpoint.Configure(o => { o.PortName = "COM1"; o.BaudRate = 115200; });
        endpoint.UseProtocol<ModbusProtocol>();
        endpoint.UseReconnect<DefaultReconnectStrategy>();
        endpoint.UsePipeline(p => p.Use<LoggingMiddleware>());
        endpoint.AddHandler<PlcHandler>();
    });
});
services.AddSerialHostedService();
```

### 普通模式
```csharp
var manager = new SerialManager();
manager.AddEndpoint("PLC", endpoint => { ... });
manager.StartAllAsync();
```

## 目标框架
netstandard2.0;net8.0
