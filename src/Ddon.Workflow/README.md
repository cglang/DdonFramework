# Ddon.Workflow - 轻量级串行工作流引擎

## 概述

Ddon.Workflow 是一个轻量级的串行工作流引擎，用于按顺序驱动一组 **Step（步骤）** 完成复杂业务流程。适合设备控制、产线自动化等需要「按步骤推进 + 轮询等待条件」场景。

核心模型与常见的游戏状态机/流程控制类似：每个步骤按序执行，步骤通过返回 `StepStatus` 表明是继续等待（`Running`）还是完成（`Success`），引擎在每次 `UpdateAsync`（帧更新）中驱动当前步骤推进。

## 核心特性

- ✅ 串行步骤驱动：一次执行一个步骤，成功后自动进入下一步
- ✅ 轮询模型：步骤在 `OnUpdateAsync` 中反复执行，适合等待硬件/IO 条件
- ✅ 上下文共享：步骤间通过类型化的 `TContext` 共享数据与状态
- ✅ 生命周期钩子：`OnEnterAsync` / `OnUpdateAsync` / `OnExitAsync`
- ✅ 步骤扩展点：在工作流级或步骤级插入自定义逻辑（Enter/Exit 之后触发）
- ✅ 调度器：统一管理多个工作流实例，完成后自动移除
- ✅ DI 集成：通过 `Microsoft.Extensions.DependencyInjection` 注册
- ✅ 多目标框架：`netstandard2.0` / `net8.0`
- ✅ 可选持久化：核心与持久化解耦，需要时以子类方式挂载（`Ddon.Workflow.Persistence` 命名空间）

## 安装

通过 NuGet 添加 Ddon.Workflow 包：

```bash
dotnet add package Ddon.Workflow
```

## 核心概念

| 概念 | 说明 |
|------|------|
| `Workflow<TContext>` | 串行工作流引擎，持有 `TContext` 和步骤列表 |
| `Step<TContext>` | 抽象步骤基类，所有业务逻辑继承它 |
| `IStep<TContext>` | 步骤接口 |
| `StepStatus` | 步骤执行结果：`Running`（等待）/ `Success`（完成）/ `Failed`（失败） |
| `IWorkflowScheduler` | 调度器，统一驱动所有活跃工作流 |
| `IStepExtension<TContext>` | 步骤扩展点，在 Enter/Exit 之后执行 |
| `WorkflowBuilder<TContext>` | 通过 DI 链式构建工作流的 Builder |
| `PersistableWorkflow<TContext>` | 持久化子类：自动保存/清除检查点（`Ddon.Workflow.Persistence`） |

### 步骤生命周期

```
OnEnterAsync（进入，执行一次）
    ↓
OnUpdateAsync（轮询，每帧执行，返回 StepStatus）
    ├─ Success → OnExitAsync（清理，执行一次）→ 进入下一步骤
    │              └─ 若已推进：触发 OnStepAdvancedAsync 钩子（派生类可用）
    │              └─ 若已全部完成：触发 OnWorkflowCompletedAsync 钩子（派生类可用）
    └─ Running → 下一帧继续轮询当前步骤
```

## 快速开始

### 1. 注册服务

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();
services.AddWorkflow();

var serviceProvider = services.BuildServiceProvider();
```

### 2. 定义上下文

```csharp
public class DemoContext
{
    public string TaskNo { get; set; } = string.Empty;
    public bool IsMaterialReady { get; set; }
}
```

### 3. 定义步骤

继承 `Step<TContext>`，在 `OnUpdateAsync` 中实现轮询逻辑：

```csharp
using Ddon.Workflow;

// 步骤1：发送指令（一次性动作）
public class SendCommandStep : Step<DemoContext>
{
    public override Task<StepStatus> OnUpdateAsync(DemoContext context, CancellationToken cancellationToken)
    {
        context.IsMaterialReady = false;
        Console.WriteLine($"发送指令: {context.TaskNo}");
        return Task.FromResult(StepStatus.Success);
    }
}

// 步骤2：等待设备到位（轮询直到条件满足）
public class WaitDeviceStep : Step<DemoContext>
{
    public override Task<StepStatus> OnUpdateAsync(DemoContext context, CancellationToken cancellationToken)
    {
        if (context.IsMaterialReady)
        {
            Console.WriteLine("物料到位，任务完成");
            return Task.FromResult(StepStatus.Success);
        }
        return Task.FromResult(StepStatus.Running); // 继续等待
    }
}
```

### 4. 创建工作流并驱动

```csharp
using Ddon.Workflow;
using Ddon.Workflow.Abstractions;

var scheduler = serviceProvider.GetRequiredService<IWorkflowScheduler>();

var context = new DemoContext { TaskNo = "T101" };
var steps = new List<IStep<DemoContext>>
{
    new SendCommandStep(),
    new WaitDeviceStep()
};

var workflow = new Workflow<DemoContext>("出库任务", context, steps);

// 注册并启动（触发第一个步骤的 OnEnterAsync）
await scheduler.StartAsync(workflow);

// 主循环轮询（帧更新驱动）
while (true)
{
    await scheduler.UpdateAsync();   // 工作流完成后自动从调度器移除
    await Task.Delay(100);           // 10Hz 刷新频率
}
```

## 使用 WorkflowBuilder 构建

`WorkflowBuilder` 支持通过 DI 容器解析步骤（步骤需注册为服务）：

```csharp
// 注册步骤为服务
services.AddTransient<SendCommandStep>();
services.AddTransient<WaitDeviceStep>();

// 构建工作流
var builder = serviceProvider.GetRequiredService<WorkflowBuilder>();
var workflow = builder
    .CreateWorkflow<DemoContext>()
    .AddStep<SendCommandStep>()
    .AddStep<WaitDeviceStep>()
    .Build("出库任务", context);
```

## 内置步骤

| 步骤 | 说明 |
|------|------|
| `ActionStep<TContext>` | 执行一次委托动作后立即返回 `Success` |
| `TimeoutStep<TContext>` | 带超时检查的抽象基类（默认 300 秒），派生类通过 `IsTimeout()` 判断 |

```csharp
// ActionStep
new ActionStep<DemoContext>(ctx =>
{
    ctx.IsMaterialReady = true;
    return Task.CompletedTask;
});

// TimeoutStep 派生
public class WaitWithTimeoutStep : TimeoutStep<DemoContext>
{
    public override Task<StepStatus> OnUpdateAsync(DemoContext context, CancellationToken cancellationToken)
    {
        if (IsTimeout())
            return Task.FromResult(StepStatus.Failed);
        if (context.IsMaterialReady)
            return Task.FromResult(StepStatus.Success);
        return Task.FromResult(StepStatus.Running);
    }
}
```

## 步骤扩展点

扩展在步骤 `OnEnterAsync` / `OnExitAsync` 完成后触发，可用于埋点、日志、告警等横切逻辑。

```csharp
using System.Threading;
using Ddon.Workflow;
using Ddon.Workflow.Abstractions;

public class AuditExtension : IStepExtension<DemoContext>
{
    public Task AfterEnterAsync(IStep<DemoContext> step, DemoContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"进入步骤: {step.Name}");
        return Task.CompletedTask;
    }

    public Task AfterExitAsync(IStep<DemoContext> step, DemoContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"退出步骤: {step.Name}");
        return Task.CompletedTask;
    }
}
```

```csharp
// 工作流级扩展（所有步骤生效）
workflow.AddExtension(new AuditExtension());

// 步骤级扩展（仅该步骤生效）
step.AddExtension(new AuditExtension());
```

## 调度器

`WorkflowScheduler` 负责统一管理所有活跃工作流：

- `StartAsync(workflow)` — 注册工作流并触发启动
- `UpdateAsync()` — 帧更新，驱动所有工作流推进
- `GetActiveWorkflows()` — 获取当前活跃的工作流列表
- 工作流完成后自动从调度器移除

## 持久化（可选）

持久化与核心引擎解耦：核心 `Workflow` / `WorkflowScheduler` 完全不感知持久化，持久化逻辑收拢在 `Ddon.Workflow.Persistence` 命名空间，通过 `PersistableWorkflow<TContext>` 子类挂载到核心的生命周期钩子（`OnStepAdvancedAsync` / `OnWorkflowCompletedAsync`）上。

| 组件 | 说明 |
|------|------|
| `IWorkflowCheckpoint` | 检查点数据结构 |
| `IWorkflowPersistenceStrategy` | 持久化策略接口（可自定义实现，如数据库） |
| `FileSystemPersistenceStrategy` | 内置文件系统 JSON 存储策略 |
| `PersistableWorkflow<TContext>` | 支持持久化的工作流子类，自动保存/清除检查点 |
| `IWorkflowRecoveryService` | 从检查点恢复工作流的服务 |

### 1. 注册服务

```csharp
services.AddWorkflowWithPersistence("C:\\Temp\\WorkflowStorage");
```

### 2. 启用持久化

不想要持久化时继续使用普通 `Workflow`；需要持久化时改用 `PersistableWorkflow`：

```csharp
var strategy = serviceProvider.GetRequiredService<IWorkflowPersistenceStrategy>();

var workflow = new PersistableWorkflow<DemoContext>(
    "出库任务", context, steps, strategy);

await scheduler.StartAsync(workflow);
// 每成功推进一个步骤自动保存检查点，全部完成后自动清除
```

### 3. 恢复持久化工作流

应用重启后，从存储加载检查点并重建工作流，从中断的步骤继续执行：

```csharp
var recovery = serviceProvider.GetRequiredService<IWorkflowRecoveryService>();

foreach (var checkpoint in await recovery.GetRecoverableCheckpointsAsync())
{
    var recovered = await recovery.RecoverWorkflowAsync<DemoContext>(
        checkpoint,
        (stepTypeNames, ctx) => RebuildSteps(stepTypeNames, ctx)); // 重建步骤

    await scheduler.StartAsync(recovered); // 恢复后照常驱动
}
```

### 4. 自定义步骤重建

```csharp
private static IStep<DemoContext>[] RebuildSteps(string[] stepTypeNames, DemoContext context)
{
    var steps = new List<IStep<DemoContext>>();
    foreach (var typeName in stepTypeNames)
    {
        var type = Type.GetType(typeName);
        if (type != null && Activator.CreateInstance(type) is IStep<DemoContext> step)
            steps.Add(step);
    }
    return steps.ToArray();
}
```

> 注意：恢复服务会先在当前已加载程序集中解析类型名，若类型位于未加载程序集，需传入程序集限定名。

### 5. 扩展持久化策略

实现 `IWorkflowPersistenceStrategy` 即可接入任意存储（数据库、Redis 等）：

```csharp
public class DatabasePersistenceStrategy : IWorkflowPersistenceStrategy
{
    public Task SaveCheckpointAsync(IWorkflowCheckpoint checkpoint, CancellationToken cancellationToken)
    {
        // 保存到数据库
        return Task.CompletedTask;
    }
    // 其余方法实现...
}
```

## 项目结构

```
Ddon.Workflow/
├── Abstractions/               # 核心接口
│   ├── IStep.cs                #   IStep / IStep<TContext>
│   ├── IWorkflow.cs            #   IWorkflow / WorkflowBase
│   └── IWorkflowScheduler.cs   #   调度器接口
├── Persistence/                # 可选持久化（独立命名空间，核心不依赖）
│   ├── IWorkflowPersistence.cs #   IWorkflowCheckpoint / Strategy / RecoveryService
│   ├── PersistableWorkflow.cs  #   持久化工作流子类
│   ├── FileSystemPersistenceStrategy.cs
│   ├── WorkflowCheckpoint.cs
│   └── WorkflowRecoveryService.cs
├── Steps/                      # 内置步骤
│   ├── ActionStep.cs
│   └── TimeoutStep.cs
├── Microsoft/Extensions/DependencyInjection/
│   └── WorkflowExtensions.cs   # DI 注册扩展
├── IStepExtension.cs           # 步骤扩展点接口
├── Step.cs                     # 抽象步骤基类
├── StepStatus.cs               # 步骤状态枚举
├── Workflow.cs                 # 工作流引擎
├── WorkflowBuilder.cs          # 构建器
└── WorkflowScheduler.cs        # 调度器
```

## 注意事项

- **步骤不可重入**：`_done` 等步骤内部状态需自行管理，恢复/重建工作流需重新创建步骤实例
- **Failed 状态**：当前引擎不处理 `Failed`——步骤返回 `Failed` 后工作流会停在当前步骤，需由业务层自行处理异常流程
- **上下文序列化**：`TContext` 在步骤间共享，多工作流实例使用同一上下文时注意线程安全；启用持久化时 `TContext` 必须可 JSON 序列化
- **驱动方式**：引擎不自行计时，依赖外部循环周期调用 `UpdateAsync`（如主循环、`Task.Delay` 轮询或定时器）
- **检查点时机**：持久化工作流每成功推进一个步骤保存一次检查点（按步骤索引去重），全部完成后自动清除

## 设计原则

1. **单一职责** — 引擎只负责步骤推进，业务逻辑全部封装在步骤中
2. **状态轮询** — 通过 `StepStatus` 返回驱动进度，天然适配硬件/IO 等待场景
3. **可扩展** — 通过扩展点注入横切逻辑，无需修改引擎
4. **关注点分离** — 核心引擎零持久化依赖，持久化通过生命周期钩子 + 子类挂载，可按需取舍
5. **轻量依赖** — 仅依赖 Microsoft.Extensions 系列基础库

## 许可证

MIT
