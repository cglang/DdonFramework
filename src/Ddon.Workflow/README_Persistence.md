// Ddon.Workflow 持久化功能使用指南

## 概述

Ddon.Workflow 现在支持工作流执行过程中的持久化和中断后恢复功能。通过持久化，工作流可以在应用程序重启后从上次中断的地方继续执行。

## 核心组件

### 1. 持久化接口
- `IWorkflowCheckpoint`: 工作流检查点数据结构
- `IWorkflowPersistenceStrategy`: 持久化策略接口
- `IWorkflowRecoveryService`: 工作流恢复服务
- `IPersistableWorkflow`: 可持久化工作流标记接口

### 2. 内置实现
- `WorkflowCheckpoint`: 默认检查点实现
- `FileSystemPersistenceStrategy`: 基于文件系统的持久化
- `WorkflowRecoveryService`: 工作流恢复服务

## 使用方法

### 1. 配置依赖注入

```csharp
// 添加带持久化的工作流服务
services.AddWorkflowWithPersistence("C:\\Temp\\WorkflowStorage");

// 或使用自定义持久化策略
services.AddWorkflowWithCustomPersistence(new FileSystemPersistenceStrategy("path", logger));
```

### 2. 启用工作流持久化

```csharp
// 创建工作流
var workflow = new Workflow<MyContext>("MyWorkflow", context, steps);

// 启用持久化
workflow.EnablePersistence(serviceProvider.GetRequiredService<IWorkflowPersistenceStrategy>());

// 启动工作流
await scheduler.StartAsync(workflow);
```

### 3. 恢复持久化工作流

```csharp
// 获取调度器和服务
var scheduler = serviceProvider.GetRequiredService<IWorkflowScheduler>();
var recoveryService = serviceProvider.GetRequiredService<IWorkflowRecoveryService>();

// 恢复所有持久化的工作流
await scheduler.RecoverPersistedWorkflowsAsync(async checkpoint =>
{
    // 自定义恢复逻辑：重建上下文和步骤
    var recoveredContext = JsonSerializer.Deserialize<MyContext>(
        checkpoint.ContextJson,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    var recoveredSteps = RebuildSteps(checkpoint.StepTypeNames, recoveredContext);

    var recoveredWorkflow = new Workflow<MyContext>(
        checkpoint.WorkflowName,
        recoveredContext,
        recoveredSteps);

    recoveredWorkflow.Id = checkpoint.WorkflowId;
    recoveredWorkflow.RestoreCheckpoint(checkpoint.CurrentStepIndex);
    recoveredWorkflow.EnablePersistence(persistenceStrategy);

    return recoveredWorkflow;
});
```

### 4. 自定义步骤重建

```csharp
private static IStep<MyContext>[] RebuildSteps(string[] stepTypeNames, MyContext context)
{
    var steps = new List<IStep<MyContext>>();

    foreach (var typeName in stepTypeNames)
    {
        var type = Type.GetType(typeName);
        if (type != null)
        {
            var step = (IStep<MyContext>)Activator.CreateInstance(type);
            // 根据需要初始化步骤状态
            steps.Add(step);
        }
    }

    return steps.ToArray();
}
```

## 工作原理

1. **检查点创建**: 工作流在步骤成功完成后自动创建检查点
2. **状态持久化**: 当前步骤索引、上下文数据和步骤类型信息被序列化并保存
3. **恢复过程**: 应用程序启动时，从存储中加载检查点并重建工作流状态
4. **继续执行**: 恢复的工作流从中断的步骤继续执行

## 注意事项

- **序列化要求**: 上下文类型必须可序列化为JSON
- **类型解析**: 步骤类型必须在运行时可解析（确保程序集可用）
- **并发安全**: 多实例部署时需考虑存储一致性
- **清理机制**: 完成的工作流会自动删除检查点

## 扩展持久化策略

可以实现自定义的持久化策略，例如数据库存储：

```csharp
public class DatabasePersistenceStrategy : IWorkflowPersistenceStrategy
{
    // 实现数据库持久化逻辑
    public async Task SaveCheckpointAsync(IWorkflowCheckpoint checkpoint, CancellationToken cancellationToken)
    {
        // 保存到数据库
    }

    // 其他方法实现...
}
```