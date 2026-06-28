# AI 上下文记录

本文档记录 AI 助手在与该代码库交互过程中了解和操作的上下文信息。

## 当前会话上下文

### 最近操作
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
- `PipelineBuilder.Use(Func<SerialContext, Task>)` 方法签名已正确调整（移除了不必要的泛型约束）
- `UseBackgroundService()` 方法已移除（设计文档有提及但当前架构通过 SerialHostedService 统一管理）
- 包版本冲突已解决：通过移除显式 Microsoft.Extensions.* 引用，依赖 Ddon.Pipeline 传递解析

## 代码库约定速查
| 约定 | 说明 |
|------|------|
| 代码无注释 | 项目中代码文件不写注释 |
| 文件名大小写 | PascalCase |
| 命名空间 | 与文件夹路径一致 |
| DI 扩展命名空间 | Microsoft.Extensions.DependencyInjection |
| 属性文件 | common.props + version.props 分离 |
