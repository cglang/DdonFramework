# AI 上下文记录

本文档记录 AI 助手在与该代码库交互过程中了解和操作的上下文信息。

## 当前会话上下文

### 最近操作
- **Ddon.Pipeline 修复**: 修复 6 个问题（2025-06）
  1. `MoveNext()` 反向迭代 → 前向迭代（符合 IEnumerator 契约）
  2. `Reset()` 设 `_curIndex = -1`（原为 `Count`，越界）
  3. `AddMiddleware` 设 `_curIndex = Count - 1`（原为 `Count`）
  4. `Dispose()` 移除 `GC.SuppressFinalize`（无终结器）
  5. `ContainerPipelineInstanceProvider` 变量名 `feneralMiddleware` → `generalMiddleware`
  6. `Build()` 新增 `IPipelineInstanceProvider` 参数重载（原硬编码 DefaultPipelineInstanceProvider）
  7. `DecisionPipeline.Build<TContext>` 移除 `where TContext : new()` 约束
  - Ddon.Pipeline + Ddon.Serial + Ddon.Socket 全部构建通过（0 警告 0 错误）

- **实现 Ddon.Serial 框架**: 根据 Ddon.Serial/设计文档.md，完整实现了 24 个源文件
  - Abstractions 9 接口、Builder 3 类、Configuration 2 类
  - Core 6 实现、Models 2 类、Hosted 1 类、Extensions 1 类
  - 构建成功（netstandard2.0 + net8.0，0 警告 0 错误）

- **实现 Ddon.Socket 框架**: 按照 Ddon.Serial 同构架构重新实现
  - 22 源文件，与 Serial 架构一一对应
  - Server 模式：新增 `SocketServer`（TcpListener + accept 循环）
  - Client 模式：`SocketWorker(TcpClient)` 构造支持已接受的连接
  - SocketEndpointBuilder：`Build(TcpClient)` 重载用于 Server 模式创建 Endpoint
  - 构建成功（netstandard2.0 + net8.0，0 警告 0 错误）

- **Ddon.Socket 断线自动重连**: 实现 Client 模式断线自动重连
  - 新增 `DefaultReconnectStrategy` — 指数退避（1s→2s→4s→8s→15s→封顶 30s）
  - `SocketEndpoint.OnDisconnected` — 触发 `ReconnectLoopAsync` 后台重连
  - `SocketWorker.ConnectAsync` — 修复 `_receiveCts` 不可复用的 bug（dispose 旧对象，创建新的）
  - `SocketEndpointBuilder.Build(TcpClient)` — Server 端接受连接的 Endpoint 不重连
  - `StopAsync` — 先 cancel 再 await 重连任务，确保干净退出
  - 构建成功（0 警告 0 错误）

- **重构 Ddon.OpenProtocol**: 完全重写 Open Protocol TCP Client（2026-06）
  - 删除旧 DdonOPClient 目录（6 个旧文件）
  - 按 Ddon.Socket 同构七层结构重构（25 个新文件）
  - TCP 传输复用 Ddon.Socket 的 `ISocketWorker`
  - 5 个核心问题修复：
    1. NUL 终止符写死 → `MessageTerminator` 枚举（None/Nul/CrLf/Custom）
    2. Mid0004 无法处理 → `RequestResponseMatcher` 自动路由 + `OpenProtocolException`
    3. 订阅结果不可等待 → `SubscribeAsync<T>` 首个响应 Task
    4. 命名空间/结构混乱 → `Ddon.OpenProtocol.*` 统一 + 七层结构
    5. 自定义 MID 不便捷 → `RegisterCustomMid<T>()` 类型驱动
  - 构建成功（netstandard2.0 + net8.0，0 警告 0 错误）

- **生成 Ddon.VitrinPLC 上下文文档**: 读取全部 34 源文件，生成 `.ai/doc/DDON_VITRINPLC.md`（2026-07）
  - 四层架构、数据流、使用示例、地址格式、已知问题

- **Ddon.VitrinPLC 依赖解耦**: PlcSyncEngine 改为依赖接口（2026-07）
  - `IPlcMemoryMirror` 接口新增 `ApplySnapshot` / `GetRegionInfo` 方法
  - `PlcSyncEngine` 字段和构造函数参数 `PlcMemoryMirror` → `IPlcMemoryMirror`
  - 构建成功（0 警告 0 错误）

- **Ddon.VitrinPLC 运行时 Tag/PLC 动态管理**: 提升 Tag 和 PLC 为运行时对象（2026-07）
  - Tag 运行时化：`ITagRegistry` 新增 `Unregister` / `TagRegistered` / `TagUnregistered`；`IPlcSession` 新增 `AddTag` / `RemoveTag` / `Tags`
  - PLC 运行时化：`IPlcHub` 新增 `AddPlcAsync` / `RemovePlcAsync`；`PlcHub` 改为 `ConcurrentDictionary`
  - `PlcSyncEngine` 监听 `TagRegistered` 事件动态扩展区域
  - 提取 `PlcServiceFactory`（`ActivatorUtilities.CreateInstance`），解决 DI 手动构造问题
  - 修复 `PlcHub.AddPlcAsync` 连接失败时回滚注册
  - 构建成功（0 警告 0 错误），测试项目运行验证通过

### Ddon.Serial 关键设计决策
1. Pipeline 使用 Ddon.Pipeline（设计文档明确要求）
2. 每个 Endpoint 完全独立（Worker/Pipeline/Dispatcher/Protocol/Handler）
3. Worker 使用 `SerialPort.BaseStream.ReadAsync` 实现真正异步 IO
4. 协议解码支持内部缓冲（`_receiveBuffer`），处理不完整帧
5. Manager 仅管理 Endpoint 生命周期，不参与数据处理
6. Dispatcher 是 Pipeline 之后的独立步骤（非终端 Middleware）
7. Factory 优先从 DI 容器解析，回退到 Activator.CreateInstance
8. DI 注册采用 Singleton SerialManager + 可选 HostedService

### 已知问题/待办
- Ddon.OpenProtocol 已重构完成（0 警告 0 错误），无待办
- Ddon.VitrinPLC 架构审查问题 #1、#2、#3 已修复，剩余 3 项待处理

## 代码库约定速查
| 约定 | 说明 |
|------|------|
| 代码无注释 | 项目中代码文件不写注释 |
| 文件名大小写 | PascalCase |
| 命名空间 | 与文件夹路径一致 |
| DI 扩展命名空间 | Microsoft.Extensions.DependencyInjection |
| 属性文件 | common.props + version.props 分离 |
