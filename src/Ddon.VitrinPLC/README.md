# PLC 统一内存镜像架构 — C# 实现

## 架构总览

```
┌─────────────────────────────────────────────────┐
│  业务层  UI / MES / SCADA / Dashboard           │
│  ↓ 调用 ITagService.Get<T> / SetAsync<T>        │
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
└─────────────────────────────────────────────────┘
```

## 项目结构

```
PlcMirror/
├── Plc.Core/
│   ├── Interfaces.cs       # 所有核心接口定义
│   ├── Models.cs           # TagDefinition, WriteResult, TagChange, MemoryRegion
│   ├── AddressParser.cs    # 统一地址解析（S7/MC/FINS/Modbus）
│   └── PlcCodec.cs         # byte[] ↔ 强类型编解码（Big-Endian）
│
├── Plc.MemoryMirror/
│   └── PlcMemoryMirror.cs  # 只读镜像实现，原子区域替换
│
├── Plc.TagEngine/
│   ├── TagRegistry.cs      # Tag 注册与解析
│   ├── TagService.cs       # 业务入口（Get/Set/Subscribe）
│   └── ChangeNotifier.cs   # 订阅/发布变化通知
│
├── Plc.SyncEngine/
│   ├── PlcSyncEngine.cs    # 周期扫描引擎
│   └── WriteCommandService.cs # 直接写 PLC
│
├── Plc.Protocol.Siemens/
│   └── SiemensClient.cs    # S7 协议（COTP + S7 PDU）
│
├── Plc.Protocol.Mitsubishi/
│   └── MitsubishiClient.cs # MC 协议（3E 帧，二进制）
│
├── Plc.Protocol.Omron/
│   └── OmronClient.cs      # FINS/TCP 协议
│
├── Plc.Hosting/
│   └── PlcMirrorExtensions.cs # DI 扩展 + IHostedService
│
└── Program.cs              # 完整使用示例
```

## 四大设计原则

| # | 原则 | 说明 |
|---|------|------|
| 1 | PLC 是真实源 | 任何状态最终以 PLC 当前值为准 |
| 2 | 内存镜像只读 | 业务写操作不修改本地镜像 |
| 3 | 写入立即下发 | `SetAsync` 直接写 PLC，不经过镜像 |
| 4 | 下周期再反映 | UI 在 200ms 后才看到新值，是刻意的一致性换延迟 |

## 写入流程（时序）

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

## 快速开始

```csharp
services.AddPlcMirror(x =>
{
    x.UseSiemens("Main", plc =>
    {
        plc.Ip   = "192.168.1.10";
        plc.Port = 102;
    });
    x.ScanInterval = 200;
    x.MapTag("Temp",  "DB1.DBD0",    PlcDataType.Float);
    x.MapTag("Run",   "DB1.DBX10.0", PlcDataType.Bool);
    x.MapTag("Count", "D100",        PlcDataType.Int16);
});

// 读（无 IO）
float temp = tags.Get<float>("Temp");

// 写（直接发 PLC）
var r = await tags.SetAsync("Run", true);

// 订阅变化
tags.Subscribe<float>("Temp", v => Console.WriteLine($"温度: {v}"));
```

## 支持地址格式

| 品牌 | 示例地址 | 说明 |
|------|----------|------|
| 西门子 | `DB1.DBD0` | DB 块双字 |
| 西门子 | `DB1.DBX10.5` | DB 块位 |
| 西门子 | `DB1.DBW4` | DB 块字 |
| 西门子 | `M0.0` / `MW10` / `MD20` | M 区位/字/双字 |
| 三菱 | `D100` / `M200` / `X0` | D/M/X/Y 区 |
| 欧姆龙 | `D100` / `W0` / `CIO0` | DM/W/CIO 区 |
| Modbus | `400001` / `000001` | 保持寄存器/线圈 |

## 不适用场景

> ⚠️ 此架构**不适合**以下场景，这些必须在 PLC 本地执行：
> - 高频运动控制
> - 毫秒级联锁
> - 安全停机控制（SIL）
