# Ddon.VitrinPLC — PLC 统一内存镜像框架

## 用途
多 PLC 统一内存镜像框架，支持同时连接多台不同品牌的 PLC，通过周期扫描将 PLC 内存映射到本地镜像，提供零延迟读取、直接写入和值变化订阅功能。

## 设计目标
- 多 PLC 并发管理，每台独立 SyncEngine、Mirror、Session
- 内存镜像读取（零 IO 延迟）
- 写入直接下发 PLC，不污染镜像
- 支持 Siemens、Mitsubishi、Omron 三种主流 PLC 协议
- 支持扩展自定义 IPlcClient / IPlcClientFactory
- 集成 Microsoft DI 和 Generic Host

## 四层架构

```
业务层      IPlcHub.For("name") → IPlcSession
  ↓
Tag API     PlcSession  →  Get<T> / SetAsync / Subscribe
  ↓
内存镜像    PlcMemoryMirror  →  byte[] 区域快照（只读）
  ↓
同步引擎    PlcSyncEngine  →  周期扫描 PLC → 原子替换镜像 → 变化检测 → 通知
  ↓
协议层      IPlcClient  →  SiemensClient / MitsubishiClient / OmronClient
```

## 四大设计原则

| # | 原则 | 说明 |
|---|------|------|
| 1 | PLC 是真实源 | 任何状态最终以 PLC 当前值为准 |
| 2 | 内存镜像只读 | 业务写操作不修改本地镜像 |
| 3 | 写入立即下发 | `SetAsync` 直接写 PLC，不经过镜像 |
| 4 | 下周期再反映 | UI 在下个扫描周期才看到新值，以延迟换一致性 |

## 项目结构 (34 源文件)

```
Ddon.VitrinPLC/
├─ Abstractions/              (9 接口)
│  ├─ IPlcHub                 多 PLC 访问入口，支持运行时 AddPlcAsync / RemovePlcAsync
│  ├─ IPlcSession             业务层入口：Get/SetAsync/Subscribe，支持运行时 AddTag/RemoveTag
│  ├─ IPlcMemoryMirror        内存镜像：版本号、区域读取、ApplySnapshot、GetRegionInfo、RegisterRegion
│  ├─ IPlcSyncEngine          同步引擎：Start/Stop/ScanOnce
│  ├─ IPlcClient              协议层抽象：Connect/Disconnect/ReadBytes/WriteBytes
│  ├─ IPlcClientFactory       自定义客户端工厂
│  ├─ ITagRegistry             Tag 注册表：Register/Unregister/Resolve/GetAll + TagRegistered/TagUnregistered 事件
│  ├─ IChangeNotifier          变化通知：Subscribe/NotifyChanges
│  └─ IWriteCommandService    写命令服务
│
├─ Clients/                   (3 实现)
│  ├─ SiemensClient + SiemensOptions    基于 S7.Net（S7-1500）
│  ├─ MitsubishiClient + MitsubishiOptions   MC 协议 3E 帧（手写二进制协议）
│  └─ OmronClient + OmronOptions        FINS/TCP 协议（手写二进制协议）
│
├─ SyncEngine/                (2 实现)
│  ├─ PlcSyncEngine           核心：按区域批量读 → 替换镜像 → 变化检测 → 事件发布。监听 TagRegistered 自动扩展区域
│  └─ WriteCommandService     直接写 PLC，不修改镜像
│
├─ TagEngine/                 (3 实现)
│  ├─ PlcSession              业务入口：Get(读镜像) / SetAsync(写PLC) / Subscribe / AddTag / RemoveTag
│  ├─ TagRegistry             ConcurrentDictionary<string, TagDefinition>，支持 Unregister 及事件
│  └─ ChangeNotifier          基于 Subscription 的变化通知，Dispose 自动清理
│
├─ Models/                    (9 数据模型)
│  ├─ TagDefinition           Name/Address/Type/StringLength
│  ├─ PlcDataType             Bool/Byte/Int16/UInt16/Int32/UInt32/Float/Double/String
│  ├─ EndianFormat            ABCD/BADC/CDAB/DCBA
│  ├─ MemoryRegion            线程安全内存区域（ReaderWriterLockSlim）
│  ├─ MemoryRegionInfo        区域元信息：RegionKey/Area/StartOffset/Length
│  ├─ RegionConfig            区域配置（内部 record）
│  ├─ TagChange               OldValue/NewValue/Tag/ChangedAt
│  ├─ WriteResult             Success/NeedConfirmByScan/ErrorMessage
│  └─ ScanCompletedEventArgs  Version/Elapsed/Changes
│
├─ PlcHub.cs                 多 PLC Hub 实现，ConcurrentDictionary 管理 engines + sessions，支持运行时增删
├─ PlcMemoryMirror.cs         内存镜像实现，ApplySnapshot + 按 Tag 读取 + RegisterRegion
├─ PlcServiceFactory.cs       内部工厂，统一构造 PlcSyncEngine/PlcSession 等（ActivatorUtilities.CreateInstance）
├─ AddressParser.cs           统一地址解析（Siemens/Mitsubishi/Omron/Modbus）
├─ PlcCodec.cs                编解码：Read<T>/Encode，支持 4 种字节序
├─ VitrinPlcBuilder.cs        DSL Builder：AddSiemens/AddMitsubishi/AddOmron/AddClient
├─ PlcHostOptions.cs          单个 PLC 配置：Tags/Regions/ScanInterval/Endian
├─ PlcClientType.cs           品牌枚举（None/Siemens/Mitsubishi/Omron）
├─ VitrinPlcHostedService.cs  IHostedService 包装，启动/停止所有引擎
└─ PlcMirrorServiceCollectionExtensions.cs  DI 注册：AddVitrinPlc()
```

## 数据流

### 读取流程（同步，零 IO）
```
PlcSession.Get<T>("Temp")
  → TagRegistry.Resolve("Temp")          // 查 Tag 定义
  → AddressParser.Parse("DB1.DBD0", Float)
  → PlcMemoryMirror.Read<T>(tag)         // 从本地 byte[] 解码
  → PlcCodec.Read<float>(snapshot, addr) // 字节 → 值
```

### 扫描同步流程（每 200ms）
```
PlcSyncEngine.ScanOnceAsync()
  → GroupTagsByRegion(tags)              // 按 RegionKey 分组
  → 每区域:
      → IPlcClient.ReadBytesAsync(area, minOff, length)  // 批量读 PLC
      → PlcMemoryMirror.ApplySnapshot(regionKey, newData) // 原子替换镜像
      → 逐 Tag 新旧值比较                            // 变化检测
  → IChangeNotifier.NotifyChanges(changes)             // 发布变化事件
  → ScanCompleted 事件触发
```

### 写入流程（异步，直接写 PLC）
```
PlcSession.SetAsync("Run", true)
  → TagRegistry.Resolve("Run")
  → WriteCommandService.ExecuteAsync()
      → AddressParser.Parse("DB1.DBX10.0", Bool)
      → PlcCodec.Encode(true, Bool, endian)
      → IPlcClient.WriteBytesAsync(address, bytes)   // 直接写 PLC
      → 返回 WriteResult { Success, NeedConfirmByScan=true }
  // 镜像不变，下个扫描周期自动更新
```

## 使用示例

### DI 注册（多 PLC）
```csharp
services.AddVitrinPlc(builder =>
{
    builder.AddSiemens("main",
        c => { c.Ip = "192.168.1.10"; c.Port = 102; },
        h =>
        {
            h.ScanInterval = 200;
            h.MapTag("Temp", "DB1.DBD0", PlcDataType.Float);
            h.MapTag("Run", "DB1.DBX10.0", PlcDataType.Bool);
        });

    builder.AddMitsubishi("sub",
        c => { c.Ip = "192.168.1.20"; },
        h =>
        {
            h.MapTag("Speed", "D100", PlcDataType.Int16);
            h.MapTag("Alarm", "M200", PlcDataType.Bool);
        });

    builder.AddOmron("omron",
        c => { c.Ip = "192.168.1.30"; c.Port = 9600; },
        h => { h.MapTag("Pressure", "D50", PlcDataType.Float); });
});
```

### 业务使用
```csharp
public class PlcService(IPlcHub hub)
{
    public float ReadTemp() => hub.For("main").Get<float>("Temp");   // 零 IO
    public async Task StartAsync() => await hub.For("main").SetAsync("Run", true);
    public void Watch() => hub.For("main").Subscribe<float>("Temp", v => Console.WriteLine(v));
}
```

### 自定义客户端注册
```csharp
builder.AddClient("custom", new MyPlcClient("10.0.0.1"), h =>
{
    h.MapTag("Valve", "X0", PlcDataType.Bool);
});

// 或通过工厂
builder.AddClientFactory("custom2", new MyPlcClientFactory("10.0.0.1"), h =>
{
    h.MapTag("Motor", "Y0", PlcDataType.Bool);
});
```

## 支持地址格式

| 品牌 | 示例 | 说明 |
|------|------|------|
| Siemens | `DB1.DBD0` / `DB1.DBX10.5` / `DB1.DBW4` / `DB1.DBB8` | DB 块 |
| Siemens | `M0.0` / `MW10` / `MD20` | M 区 |
| Mitsubishi | `D100` / `M200` / `X0` / `Y0` | 三菱 MC 协议 |
| Omron | `D100` / `W0` / `CIO0` | FINS 协议 |
| Modbus | `400001` / `000001` | 保持寄存器/线圈 |

## 内置 PLC 协议

| 客户端 | 依赖库 | 协议 | 说明 |
|--------|--------|------|------|
| SiemensClient | S7netplus (S7.Net) | S7 ISO-on-TCP | 基于第三方库，支持 S7-1500 |
| MitsubishiClient | — | MC 协议 3E 帧（二进制） | 手写二进制协议，TcpClient |
| OmronClient | — | FINS/TCP | 手写二进制协议，含握手 |

## 字节序

每种 PLC 品牌预设默认字节序，用户可在 `PlcHostOptions.Endian` 中覆盖：

| 品牌 | 默认字节序 |
|------|----------|
| Siemens | ABCD (Big-Endian) |
| Mitsubishi | DCBA (Little-Endian) |
| Omron | CDAB |

## 运行时动态管理（v7.0.16+）

### Tag 运行时增删
```csharp
var session = hub.For("main");

// 运行时添加 Tag
session.AddTag(new TagDefinition("Pressure", "DB1.DBD8", PlcDataType.Float));
float val = session.Get<float>("Pressure");

// 运行时移除 Tag
session.RemoveTag("Pressure");
```

### PLC 运行时增删
```csharp
// 运行时添加新 PLC
await hub.AddPlcAsync("line2", new SiemensClient(opts, logger), h =>
{
    h.MapRegion("DB1", "DB1", 0, 512);
    h.MapTag("LineSpeed", "DB1.DBW0", PlcDataType.Int16);
});

// 为新 PLC 运行时添加 Tag
hub.For("line2").AddTag(new TagDefinition("LineTemp", "DB1.DBD4", PlcDataType.Float));

// 运行时移除 PLC
await hub.RemovePlcAsync("line2");
```

## 已知问题（来自架构审查 2026-06-23）

3. **缺少重连策略**：连接断开仅 log + retry，无指数退避
4. **ChangeNotifier 无批次边界**：每个 Tag 单独触发回调，无法获知一次扫描的完整变化集
5. **添加新协议需修改 Builder**：缺乏插件式注册机制

## 不适用场景

- 高频运动控制
- 毫秒级联锁
- 安全停机控制（SIL）

## 目标框架

net8.0

## 依赖项

- Microsoft.Extensions.Hosting.Abstractions
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging.Abstractions
- S7netplus（西门子协议）
- System.Text.Json
