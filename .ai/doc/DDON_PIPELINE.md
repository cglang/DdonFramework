# Ddon.Pipeline 管道框架

## 用途
通用的中间件管道框架，支持注册式中间件链，被 Socket、Serial、UniPLC 等模块复用。

## 核心接口

```csharp
// 管道上下文泛型接口
IGeneralCustomPipeline<TContext>
  └─ Task ExecuteAsync(TContext context)

// 中间件接口
IGeneralPipelineMiddleware<TContext>
  ├─ int Index { get; set; }
  └─ Task InvokeAsync(TContext context, PipelineDelegate<TContext> next)

// 管道委托
PipelineDelegate<TContext>  = delegate Task(TContext)

// 注册器
IPipelineRegistrar<TContext> : IEnumerator<IGeneralPipelineMiddleware<TContext>>
  ├─ AddMiddleware<TMiddleware>()
  ├─ AddMiddleware(Func<TContext, Task>)
  └─ AddMiddleware(Func<TContext, Task>, Func<TContext, Task>)

// 实例提供器
IPipelineInstanceProvider<TContext>
  └─ IGeneralPipelineMiddleware<T> GetInstance(Type type)
```

## 核心类

- **GeneralCustomPipelineFactory\<T\>** — 静态工厂，创建 PipelineBuild
- **GeneralCustomPipelineBuild\<T\>** — 构造器，链式配置中间件后 Build()（支持可选的 IPipelineInstanceProvider）
- **GeneralCustomPipeline\<T\>** — 最终管道实现
- **PipelineRegistrar\<T\>** — 默认注册器实现（IEnumerator 前向迭代）
- **DefaultPipelineInstanceProvider\<T\>** — Activator 创建中间件实例
- **ContainerPipelineInstanceProvider\<T\>** — 从 DI 容器解析中间件

## PipelineRegistrar 注意事项

`PipelineRegistrar<TContext>` 实现 `IEnumerator<IGeneralPipelineMiddleware<TContext>>`。枚举按**前向**（插入顺序）进行：

- `MoveNext()` 递增 `_curIndex`（从 -1 开始到 `Count - 1`）
- `Reset()` 设 `_curIndex = -1`
- `Build()` 调用链依次包裹：中间件 0 包裹 FinalMiddleware，中间件 1 包裹中间件 0...最终中间件在最外层运行

`PipelineRegistrar.Build()` 有两个重载：
- `Build()` — 使用 `DefaultPipelineInstanceProvider`（Activator 创建）
- `Build(IPipelineInstanceProvider<TContext>)` — 使用自定义实例提供器（如 DI 容器）

## 使用方式

### 直接构建
```csharp
var pipeline = GeneralCustomPipelineFactory<DataContext>
    .CreatePipelineBuild()
    .ConfigureMiddlewares(s => s.AddMiddleware<MyMiddleware>())
    .Build();

await pipeline.ExecuteAsync(context);
```

### DI 注册
```csharp
services.AddBasePipeline();
```

### 中间件定义
```csharp
public class MyMiddleware : IGeneralPipelineMiddleware<MyContext>
{
    public int Index { get; set; }
    public async Task InvokeAsync(MyContext ctx, PipelineDelegate<MyContext> next)
    {
        // 前置处理
        await next(ctx);
        // 后置处理
    }
}
```

## 目标框架
netstandard2.0;net8.0
