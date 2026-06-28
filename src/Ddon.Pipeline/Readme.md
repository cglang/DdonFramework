# Ddon.Pipeline

通用中间件管道框架，支持注册式中间件链。被 Ddon.Serial、Ddon.Socket 等模块复用。

## 核心类型

| 类型 | 用途 |
|------|------|
| `IGeneralCustomPipeline<TContext>` | 管道接口：`Task ExecuteAsync(TContext)` |
| `GeneralCustomPipeline<TContext>` | 管道默认实现 |
| `IGeneralPipelineMiddleware<TContext>` | 中间件接口：`InvokeAsync(ctx, next)` |
| `PipelineDelegate<TContext>` | 管道委托：`delegate Task(TContext)` |
| `IPipelineRegistrar<TContext>` | 中间件注册器（实现 `IEnumerator<T>`，前向迭代） |
| `PipelineRegistrar<TContext>` | 注册器默认实现 |
| `IPipelineInstanceProvider<TContext>` | 中间件实例提供器 |
| `DefaultPipelineInstanceProvider<TContext>` | 默认提供器（Activator.CreateInstance） |
| `ContainerPipelineInstanceProvider<TContext>` | DI 容器提供器（从 IServiceProvider 解析） |
| `GeneralCustomPipelineFactory<TContext>` | 管道工厂：创建 PipelineBuild |
| `GeneralCustomPipelineBuild<TContext>` | 管道构造器：链式配置后 Build() |
| `DefaultGeneralPipelineMiddleware<TContext>` | 匿名中间件包装，支持异常封装 |
| `DecisionPipeline` | 条件决策管道工厂 |

## 基础用法

### 直接构建

```csharp
var pipeline = GeneralCustomPipelineFactory<DataContext>
    .CreatePipelineBuild()
    .ConfigureMiddlewares(s =>
    {
        s.AddMiddleware<LogMiddleware>();
        s.AddMiddleware<ValidateMiddleware>();
        s.AddMiddleware(async ctx =>
        {
            // 仅前置操作
        });
        s.AddMiddleware(
            async ctx => { /* 前置 */ },
            async ctx => { /* 后置 */ }
        );
    })
    .Build();

await pipeline.ExecuteAsync(new DataContext());
```

### 中间件定义

```csharp
public class LogMiddleware : IGeneralPipelineMiddleware<DataContext>
{
    public int Index { get; set; }

    public async Task InvokeAsync(DataContext ctx, PipelineDelegate<DataContext> next)
    {
        Console.WriteLine($"Before: {ctx.Context}");
        await next(ctx);
        Console.WriteLine($"After: {ctx.Context}");
    }
}
```

### 上下文定义

```csharp
public class DataContext
{
    public string Context { get; set; } = string.Empty;
}
```

## 依赖注入

```csharp
// Startup / Program
services.AddBasePipeline();

// AddBasePipeline 注册了：
//   Singleton  IPipelineInstanceProvider<>  → ContainerPipelineInstanceProvider<>
//   Transient  IPipelineRegistrar<>          → PipelineRegistrar<>
//   Transient  IGeneralCustomPipeline<>      → GeneralCustomPipeline<>
```

通过 DI 注册自定义管道（适用于高阶场景）：

```csharp
public interface IMyPipeline : IGeneralCustomPipeline<MyContext> { }

public class MyPipeline : GeneralCustomPipeline<MyContext>, IMyPipeline
{
    public MyPipeline(IPipelineRegistrar<MyContext> registrar) : base(registrar)
    {
    }
}

// 注册器
public interface IMyPipelineRegistrar : IPipelineRegistrar<MyContext> { }

public class MyPipelineRegistrar : PipelineRegistrar<MyContext>, IMyPipelineRegistrar
{
    public MyPipelineRegistrar(IPipelineInstanceProvider<MyContext> provider) : base(provider) { }
}
```

## 自定义实例提供器

```csharp
var provider = new ContainerPipelineInstanceProvider<MyContext>(serviceProvider);

var pipeline = GeneralCustomPipelineFactory<MyContext>
    .CreatePipelineBuild()
    .ConfigureMiddlewares(s => s.AddMiddleware<MyMiddleware>())
    .Build(provider);  // 传入自定义提供器
```

## 条件决策管道

```csharp
var pipeline = DecisionPipeline.Build(
    new DecisionPipelineMiddleware<MyContext>(
        decideFunc:     async ctx => ctx.Value > 0,
        logicBeforeFunc: async ctx => Console.WriteLine("Before"),
        logicFunc:      async ctx => Console.WriteLine("Execute"),
        logicAfterFunc:  async ctx => Console.WriteLine("After")
    ),
    new DecisionPipelineMiddleware<MyContext>(
        decideFunc: async ctx => ctx.Value == 0,
        logicFunc:  async ctx => Console.WriteLine("Zero")
    )
);
await pipeline.ExecuteAsync(context);
```

## PipelineRegistrar 行为

`PipelineRegistrar<TContext>` 作为 `IEnumerator<T>`，按**前向（插入顺序）**迭代：

- 初始 `_curIndex = -1`
- `MoveNext()` 递增，访问 `_middlewareInstances[++_curIndex]`
- `Reset()` 设回 `-1`
- `Build()` 从索引 0 开始依次包裹中间件

## 目标框架

netstandard2.0;net8.0
