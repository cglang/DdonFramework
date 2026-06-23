# Ddon.VitrinPLC 架构审查记录

## 审查信息

- **项目**: `Ddon.VitrinPLC`
- **日期**: 2026-06-23
- **范围**: 核心架构、分层设计、依赖注入、代码质量

---

## 架构总结

四层架构设计：**Protocol Layer** → **Memory Mirror Layer** → **Sync Engine Layer** → **Tag/Application Layer**。数据流单向（PLC → 镜像 → 应用），读写分离，整体结构合理。

---

## 发现问题

### 🔴 高优先级

| # | 问题 | 文件 | 说明 |
|---|------|------|------|
| 1 | **PlcSyncEngine 依赖具体类而非接口** | `SyncEngine/PlcSyncEngine.cs:27-28` | 构造函数取 `PlcMemoryMirror` 而非 `IPlcMemoryMirror`，导致无法 Mock 测试 |
| 2 | **PlcHub 内部存具体类** | `PlcHub.cs:16-17` | `List<PlcSyncEngine>` 应为 `List<IPlcSyncEngine>`，耦合实现 |
| 3 | **DI 注册代码手动 new 对象图** | `PlcMirrorServiceCollectionExtensions.cs:44-108,149-183` | `AddPlcMirror` 和 `AddVitrinPlc` 两路重复手动构造所有依赖（TagRegistry、ChangeNotifier、WriteCommandService 等），违反 DI 原则。新增依赖需两处同步修改 |
| 4 | **OmronClient / MitsubishiClient 无真实实现** | `Clients/OmronClient.cs`, `Clients/MitsubishiClient.cs` | 仅有骨架代码，所有协议逻辑注释掉，运行时报错 |

### 🟡 中优先级

| # | 问题 | 文件 | 说明 |
|---|------|------|------|
| 5 | **PlcSyncEngine 每周期分配字典** | `SyncEngine/PlcSyncEngine.cs:206-209` | `GetRegisteredLength` 每次扫描调用 `GetRegionInfo()` 创建新 `Dictionary`，高频下 GC 压力大 |
| 6 | **PlcType 与 PlcClientType 冗余** | `PlcClientType.cs:4-21` | `PlcType` 静态字符串常量类与 `PlcClientType` 枚举表达同一概念 |
| 7 | **PlcMirrorOptions 命名空间不一致** | `PlcMirrorOptions.cs:8` | 位于 `Plc.Hosting` 而非 `Ddon.VitrinPLC`，与项目其余类型不一致 |
| 8 | **ChangeNotifier 弱引用的注释误导** | `TagEngine/ChangeNotifier.cs:14` | 注释写"弱引用管理"，实际用 `volatile bool` + 手动清理，非 `WeakReference` |
| 9 | **PlcSyncEngine 无重连策略** | `SyncEngine/PlcSyncEngine.cs:86-89` | 连接断开仅 log + retry，无指数退避或连接状态机 |
| 10 | **MemoryRegion 长度校验与 PadOrTrim 矛盾** | `SyncEngine/PlcSyncEngine.cs:128`, `Models/MemoryRegion.cs:45` | Region 要求长度精确匹配，但 SyncEngine 用 `PadOrTrim` 掩盖长度差异，可能隐藏配置错误 |

### 🟢 低优先级

| # | 问题 | 文件 | 说明 |
|---|------|------|------|
| 11 | 变化通知无批次边界 | `TagEngine/ChangeNotifier.cs:44-64` | 每个 Tag 单独触发回调，订阅者无法获知一次扫描的完整变化集 |
| 12 | 客户端注册需硬编码分支 | `VitrinPlcBuilder.cs:39-82` | 添加新 PLC 协议需修改 `VitrinPlcBuilder`，缺乏插件式注册机制 |

---

## 改进建议（按优先级）

1. **DI 重构**: 将 TagRegistry、ChangeNotifier、MemoryMirror 等注册为独立 Singleton，由 DI 容器自动注入，消除 `AddPlcMirror` / `AddVitrinPlc` 中的手动 `new`
2. **接口依赖**: `PlcSyncEngine` 改用 `IPlcMemoryMirror`；`PlcHub` 改用 `IPlcSyncEngine`
3. **补齐 Client 实现**: 完成 OmronClient / MitsubishiClient 的真实协议实现
4. **缓存 `GetRegisteredLength`**: 将 Region 长度缓存到字段或字典，避免每周期分配
5. **重连策略**: 在 `PlcSyncEngine` 中加入指数退避重连逻辑
6. **清理冗余**: 移除 `PlcType` 类，统一使用 `PlcClientType` 枚举
7. **统一命名空间**: 将 `PlcMirrorOptions` 移到 `Ddon.VitrinPLC`
