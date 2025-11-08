## 基础用法

``` csharp
// 使用常规自定义管道工厂创建并构建管道
var pipeline = GeneralCustomPipelineFactory<DataContext>.CreatePipelineBuild().ConfigureMiddlewares(s =>
{
    s.AddMiddleware<SampleOneMiddleware>();
    s.AddMiddleware<SampleTwoMiddleware>();

    // 使用匿名函数定义中间件
    s.AddMiddleware(
        x =>
        {
            // 更多操作
        }
    );
    s.AddMiddleware(
        x =>
        {
            // 更多操作
        },
        x =>
        {
            // 更多操作
        }
    );
}).Build();

// 创建上下文实例
var dataContext = new DataContext();
// 执行管道
await pipeline.ExecuteAsync(dataContext);

// 定义管道上下文数据模型
public class DataContext
{
    public string Context { get; set; } = string.Empty;
}

// 定义管道中间件1
public class SampleOneMiddleware : IGeneralPipelineMiddleware<DataContext>
{
    public async Task InvokeAsync(DataContext context, PipelineDelegate<DataContext> next)
    {
        // 操作1
        await next(context);
        // 后续操作1
    }
}

// 定义管道中间件2
public class SampleTwoMiddleware : IGeneralPipelineMiddleware<DataContext>
{
    public async Task InvokeAsync(DataContext context, PipelineDelegate<DataContext> next)
    {
        // 操作2
        await next(context);
        // 后续操作2
    }
}
```

## 使用依赖注入方式注册管道并使用

``` csharp
// 首先定义管道注册器
public interface ISamplePipelineRegistrar : IPipelineRegistrar<DataContext>
{
}
// 管道注册器实例
public class SamplePipelineRegistrar : PipelineRegistrar<DataContext>, ISamplePipelineRegistrar
{
    public SamplePipelineRegistrar(IPipelineInstanceProvider<DataContext> instanceProvider) : base(instanceProvider)
    {
    }
}

// 定义自定义管道
public interface ISampleCustomPipeline : IGeneralCustomPipeline<DataContext>
{
}

public class SampleCustomPipeline : GeneralCustomPipeline<DataContext>, ISampleCustomPipeline
{
    public SampleCustomPipeline(ISocketMiddlewarePipelineRegistrar pipelineRegistrar) : base(pipelineRegistrar)
    {
    }
}

// 定义管道上下文数据模型
public class DataContext
{
    public string Context { get; set; } = string.Empty;
}
```