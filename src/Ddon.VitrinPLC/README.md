# Ddon.VitrinPLC — PLC 统一内存镜像框架

## 架构总览

```
┌─────────────────────────────────────────────────┐
│  业务层  UI / MES / SCADA / Dashboard           │
│  ↓ 单 PLC: 注入 ITagService                     │
│  ↓ 多 PLC: 注入 IPlcHub → .For("name")         │
├─────────────────────────────────────────────────┤
│  Tag API 层  TagService                         │
│  ├── Get<T>    → 读内存镜像（无 IO）             │
│  ├── SetAsync  → 直接写 PLC + 返回 WriteResult   │
│  └── Subscribe → 注册变化回调                   │
├─────────────────────────────────────────────────┤
│  内存镜像层  PlcMemoryMirror（只读）             │
│  DB1 → byte[512]   DB2 → byte[256]              │
│  M   → byte[2048]  D   → byte[20000]            │
├─────────────────────────────────────────────────┤
│  同步引擎  PlcSyncEngine                        │
│  每 200ms：读PLC → 替换Mirror → 检测变化 → 通知  │
├─────────────────────────────────────────────────┤
│  协议层  IPlcClient                             │
│  SiemensClient │ MitsubishiClient │ OmronClient  │
│  （或外部自定义实现）                            │
└─────────────────────────────────────────────────┘
```

## 四大设计原则

| # | 原则 | 说明 |
|---|------|------|
| 1 | PLC 是真实源 | 任何状态最终以 PLC 当前值为准 |
| 2 | 内存镜像只读 | 业务写操作不修改本地镜像 |
| 3 | 写入立即下发 | `SetAsync` 直接写 PLC，不经过镜像 |
| 4 | 下周期再反映 | UI 在 200ms 后才看到新值，是刻意的一致性换延迟 |

---

## 单 PLC 模式（AddPlcMirror）

适合只连接一台 PLC 的场景，注入 `ITagService` 直接使用。

### 注册

```csharp
services.AddPlcMirror(x =>
{
    x.UseSiemens("Main", plc =>
    {
        plc.Ip   = "192.168.1.10";
        plc.Port = 102;
        plc.Rack = 0;
        plc.Slot = 1;
    });
    x.ScanInterval = 200;
    x.MapTag("Temp",  "DB1.DBD0",    PlcDataType.Float);
    x.MapTag("Run",   "DB1.DBX10.0", PlcDataType.Bool);
    x.MapTag("Count", "D100",        PlcDataType.Int16);
});
```

三菱 / 欧姆龙同理，将 `UseSiemens` 换成 `UseMitsubishi` 或 `UseOmron`：

```csharp
x.UseMitsubishi("Sub", plc => { plc.Ip = "192.168.1.20"; plc.Port = 5007; });
x.UseOmron("Sub",      plc => { plc.Ip = "192.168.1.30"; plc.Port = 9600; });
```

### 使用

```csharp
public class MyService(ITagService tags)
{
    public void Read()
    {
        float temp  = tags.Get<float>("Temp");
        bool  run   = tags.Get<bool>("Run");
        short count = tags.Get<short>("Count");
    }

    public async Task WriteAsync()
    {
        WriteResult r = await tags.SetAsync("Run", true);
        // r.NeedConfirmByScan == true：值将在下次扫描后反映到镜像
    }

    public void Watch()
    {
        tags.Subscribe<float>("Temp", v => Console.WriteLine($"温度变化: {v}°C"));
    }
}
```

---

## 多 PLC 模式（AddVitrinPlc）

适合同时连接多台不同品牌 PLC 的场景，注入 `IPlcHub` 按名称访问各台 PLC。

每台 PLC 拥有**独立**的 SyncEngine、MemoryMirror 和 TagService，互不干扰。

### 注册

```csharp
services.AddVitrinPlc(builder =>
{
    // 西门子
    builder.AddSiemens("main",
        c => { c.Ip = "192.168.1.10"; c.Port = 102; },
        h =>
        {
            h.ScanInterval = 200;
            h.MapTag("Temp",  "DB1.DBD0",    PlcDataType.Float);
            h.MapTag("Run",   "DB1.DBX10.0", PlcDataType.Bool);
        });

    // 三菱
    builder.AddMitsubishi("sub",
        c => { c.Ip = "192.168.1.20"; },
        h =>
        {
            h.ScanInterval = 500;
            h.MapTag("Speed",  "D100", PlcDataType.Int16);
            h.MapTag("Alarm",  "M200", PlcDataType.Bool);
        });

    // 欧姆龙
    builder.AddOmron("omron",
        c => { c.Ip = "192.168.1.30"; c.Port = 9600; },
        h =>
        {
            h.MapTag("Pressure", "D50", PlcDataType.Float);
        });
});
```

### 使用

```csharp
public class MyService(IPlcHub hub)
{
    public void Read()
    {
        float temp  = hub.For("main").Get<float>("Temp");
        short speed = hub.For("sub").Get<short>("Speed");
    }

    public async Task WriteAsync()
    {
        await hub.For("main").SetAsync("Run", true);
    }

    public void Watch()
    {
        hub.For("sub").Subscribe<short>("Speed", v => Console.WriteLine($"速度: {v}"));
    }

    public void ListAll()
    {
        foreach (var name in hub.Names)
            Console.WriteLine($"已注册 PLC: {name}");
    }
}
```

---

## 接入自定义 PLC 客户端

实现 `IPlcClient` 接口，然后通过以下任一方式注册。

### 方式一：直接传入实例

```csharp
var myClient = new MyCustomPlcClient("192.168.1.100");

services.AddVitrinPlc(builder =>
{
    builder.AddClient("custom", myClient, h =>
    {
        h.ScanInterval = 300;
        h.MapTag("Valve", "X0", PlcDataType.Bool);
        h.MapTag("Flow",  "D0", PlcDataType.Float);
    });
});
```

### 方式二：实现 IPlcClientFactory（推荐，支持延迟创建）

```csharp
// 1. 实现工厂
public class MyPlcClientFactory : IPlcClientFactory
{
    private readonly string _ip;

    public MyPlcClientFactory(string ip) => _ip = ip;

    public IPlcClient Create(string name) => new MyCustomPlcClient(name, _ip);
}

// 2. 注册
services.AddVitrinPlc(builder =>
{
    builder.AddClientFactory("custom", new MyPlcClientFactory("192.168.1.100"), h =>
    {
        h.MapTag("Valve", "X0", PlcDataType.Bool);
    });
});
```

### IPlcClient 接口定义

```csharp
public interface IPlcClient : IDisposable
{
    string Name { get; }
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    Task<byte[]> ReadBytesAsync(string area, int start, int length, CancellationToken ct = default);
    Task WriteBytesAsync(string address, byte[] data, CancellationToken ct = default);
}
```

---

## 支持地址格式

| 品牌 | 示例地址 | 说明 |
|------|----------|------|
| 西门子 | `DB1.DBD0` | DB 块双字（Float/Int32） |
| 西门子 | `DB1.DBX10.5` | DB 块第 10 字节第 5 位 |
| 西门子 | `DB1.DBW4` / `DB1.DBB8` | DB 块字 / 字节 |
| 西门子 | `M0.0` / `MW10` / `MD20` | M 区位/字/双字 |
| 三菱 | `D100` / `M200` / `X0` / `Y0` | D/M/X/Y 区 |
| 欧姆龙 | `D100` / `W0` / `CIO0` | DM/W/CIO 区 |
| Modbus | `400001` / `000001` | 保持寄存器/线圈 |

## 写入时序

```
t=0ms   SetAsync("Run", true)
         └→ 直接写 PLC
         └→ Mirror.Run = false（不变）
         └→ 返回 WriteResult { Success=true, NeedConfirmByScan=true }

t=200ms  SyncEngine.ScanOnce()
         └→ 读 PLC → PLC.Run = true
         └→ Mirror.Run = true（更新）
         └→ ChangeNotifier → Subscribe 回调触发
```

## 不适用场景

> ⚠️ 此架构**不适合**以下场景，这些必须在 PLC 本地执行：
> - 高频运动控制
> - 毫秒级联锁
> - 安全停机控制（SIL）
