using System.Text.Json;
using Ddon.Workflow;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// 测试上下文类
public class TestContext
{
    public string UserName { get; set; }
    public int Progress { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime StartTime { get; set; }
}

// 初始化步骤
public class InitStep : Step<TestContext>
{
    public InitStep() : base()
    {
        Id = "init";
        Name = "初始化";
    }

    public override Task OnEnterAsync(TestContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{Name}] 开始初始化工作流...");
        return Task.CompletedTask;
    }

    public override async Task<StepStatus> OnUpdateAsync(TestContext context, CancellationToken cancellationToken)
    {
        await Task.Delay(500, cancellationToken);
        context.UserName = "TestUser";
        context.Progress = 0;
        context.StartTime = DateTime.Now;
        Console.WriteLine($"[{Name}] 初始化完成，用户: {context.UserName}");
        return StepStatus.Success;
    }

    public override Task OnExitAsync(TestContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{Name}] 退出初始化步骤");
        return Task.CompletedTask;
    }
}

// 处理步骤
public class ProcessStep : Step<TestContext>
{
    public ProcessStep() : base()
    {
        Id = "process";
        Name = "处理";
    }

    public override Task OnEnterAsync(TestContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{Name}] 开始处理用户: {context.UserName}");
        return Task.CompletedTask;
    }

    public override async Task<StepStatus> OnUpdateAsync(TestContext context, CancellationToken cancellationToken)
    {
        // 模拟耗时处理
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(300, cancellationToken);
            context.Progress = (i + 1) * 10;
            Console.WriteLine($"[{Name}] 进度: {context.Progress}%");
        }

        context.IsCompleted = true;
        Console.WriteLine($"[{Name}] 处理完成!");
        return StepStatus.Success;
    }

    public override Task OnExitAsync(TestContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{Name}] 退出处理步骤");
        return Task.CompletedTask;
    }
}

// 完成步骤
public class CompleteStep : Step<TestContext>
{
    public CompleteStep() : base()
    {
        Id = "complete";
        Name = "完成";
    }

    public override Task OnEnterAsync(TestContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{Name}] 工作流即将完成...");
        return Task.CompletedTask;
    }

    public override async Task<StepStatus> OnUpdateAsync(TestContext context, CancellationToken cancellationToken)
    {
        await Task.Delay(200, cancellationToken);
        Console.WriteLine($"[{Name}] 工作流执行完毕!");
        Console.WriteLine($"总耗时: {(DateTime.Now - context.StartTime).TotalSeconds:F1}秒");
        return StepStatus.Success;
    }

    public override Task OnExitAsync(TestContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{Name}] 退出完成步骤");
        return Task.CompletedTask;
    }
}

public class TestProgram
{
    private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public static async Task Run(string[] args)
    {
        Console.WriteLine("=== Ddon.Workflow 持久化功能测试 ===\n");

        // 设置Ctrl+C处理
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _cts.Cancel();
            Console.WriteLine("\n收到中断信号，正在停止...");
        };

        try
        {
            // 配置依赖注入
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            // 设置持久化存储路径
            var storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkflowStorage");
            services.AddWorkflowWithPersistence(storagePath);

            var serviceProvider = services.BuildServiceProvider();

            // 获取服务
            var scheduler = serviceProvider.GetRequiredService<IWorkflowScheduler>();
            var persistenceStrategy = serviceProvider.GetRequiredService<IWorkflowPersistenceStrategy>();
            var recoveryService = serviceProvider.GetRequiredService<IWorkflowRecoveryService>();

            Console.WriteLine($"持久化存储路径: {storagePath}\n");

            // 尝试恢复之前持久化的工作流
            Console.WriteLine("正在检查是否有需要恢复的工作流...");
            await scheduler.RecoverPersistedWorkflowsAsync(async checkpoint =>
            {
                Console.WriteLine($"发现持久化工作流: {checkpoint.WorkflowName} (步骤: {checkpoint.CurrentStepIndex})");

                // 恢复上下文
                var recoveredContext = JsonSerializer.Deserialize<TestContext>(
                    checkpoint.ContextJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 重新创建步骤
                var recoveredSteps = RebuildSteps(checkpoint.StepTypeNames, recoveredContext);

                // 创建恢复的工作流
                var recoveredWorkflow = new Workflow<TestContext>(
                    checkpoint.WorkflowName,
                    recoveredContext,
                    recoveredSteps)
                {
                    Id = checkpoint.WorkflowId
                };

                // 恢复到检查点
                recoveredWorkflow.RestoreCheckpoint(checkpoint.CurrentStepIndex);
                recoveredWorkflow.EnablePersistence(persistenceStrategy);

                Console.WriteLine("工作流已恢复，准备继续执行\n");
                return recoveredWorkflow;
            });

            // 检查是否已有活跃工作流
            var activeWorkflows = scheduler.GetActiveWorkflows();
            if (activeWorkflows.Count > 0)
            {
                Console.WriteLine($"发现 {activeWorkflows.Count} 个活跃工作流，继续执行...\n");
            }
            else
            {
                // 创建新工作流
                Console.WriteLine("创建新的测试工作流...\n");

                var context = new TestContext();
                var steps = new List<IStep<TestContext>>
                {
                    new InitStep(),
                    new ProcessStep(),
                    new CompleteStep()
                };

                var workflow = new Workflow<TestContext>("持久化测试工作流", context, steps);
                workflow.EnablePersistence(persistenceStrategy);

                await scheduler.StartAsync(workflow, _cts.Token);
            }

            // 执行工作流循环
            Console.WriteLine("开始执行工作流 (按Ctrl+C可中断，重新运行程序可恢复)...\n");

            var token = _cts.Token;
            while (!token.IsCancellationRequested)
            {
                await scheduler.UpdateAsync(token);

                // 检查是否所有工作流都完成
                var workflows = scheduler.GetActiveWorkflows();
                if (workflows.Count == 0)
                {
                    Console.WriteLine("\n所有工作流执行完成!");
                    break;
                }

                await Task.Delay(100, token);
            }

            if (token.IsCancellationRequested)
            {
                Console.WriteLine("\n工作流已中断并持久化，下次运行时会自动恢复。");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n操作已取消。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n发生错误: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    // 辅助方法：根据类型名称重建步骤
    private static IStep<TestContext>[] RebuildSteps(string[] stepTypeNames, TestContext context)
    {
        var steps = new List<IStep<TestContext>>();

        foreach (var typeName in stepTypeNames)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                var step = (IStep<TestContext>)Activator.CreateInstance(type);
                steps.Add(step);
            }
            else
            {
                Console.WriteLine($"警告: 无法解析步骤类型 {typeName}");
            }
        }

        return steps.ToArray();
    }
}
