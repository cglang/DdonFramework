using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Test.Workflow
{
    internal class WorkflowCoreTest
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Workflow Core 持久化恢复测试 ===\n");

            var services = new ServiceCollection();

            services.AddLogging(x => x.AddConsole());

            services.AddWorkflow(cfg =>
            {
                cfg.UseSqlite("Data Source=workflow.db;", true);
            });

            var provider = services.BuildServiceProvider();

            var host = provider.GetRequiredService<IWorkflowHost>();

            host.RegisterWorkflow<TestWorkflow, TestData>();

            host.Start();

            // 查询是否已有运行实例
            var controller = provider.GetRequiredService<IWorkflowController>();

            var data = new TestData();

            var workflowId = await host.StartWorkflow("persist-test-workflow", 1, data);

            Console.WriteLine($"工作流已启动，ID: {workflowId}");
            Console.WriteLine("按 Ctrl+C 强制退出程序，再次运行可自动恢复。\n");

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("收到中断信号，程序退出。");
                Environment.Exit(0);
            };

            while (true)
            {
                await Task.Delay(1000);
            }
        }
    }

    // ----------------------------
    // 上下文数据
    // ----------------------------
    public class TestData
    {
        public string UserName { get; set; }
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartTime { get; set; }
    }

    // ----------------------------
    // 初始化步骤
    // ----------------------------
    public class InitStep : StepBodyAsync
    {
        public TestData Data { get; set; }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("[初始化] 开始初始化工作流...");

            await Task.Delay(500);

            Data.UserName = "TestUser";
            Data.Progress = 0;
            Data.StartTime = DateTime.Now;

            Console.WriteLine($"[初始化] 初始化完成，用户: {Data.UserName}");

            return ExecutionResult.Next();
        }
    }

    // ----------------------------
    // 处理步骤
    // ----------------------------
    public class ProcessStep : StepBodyAsync
    {
        public TestData Data { get; set; }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine($"[处理] 开始处理用户: {Data.UserName}");

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(300);

                Data.Progress = (i + 1) * 10;
                Console.WriteLine($"[处理] 进度: {Data.Progress}%");
            }

            Data.IsCompleted = true;
            Console.WriteLine("[处理] 处理完成!");

            return ExecutionResult.Next();
        }
    }

    // ----------------------------
    // 完成步骤
    // ----------------------------
    public class CompleteStep : StepBodyAsync
    {
        public TestData Data { get; set; }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("[完成] 工作流即将完成...");

            await Task.Delay(200);

            Console.WriteLine("[完成] 工作流执行完毕!");
            Console.WriteLine($"总耗时: {(DateTime.Now - Data.StartTime).TotalSeconds:F1}秒");

            return ExecutionResult.Next();
        }
    }

    // ----------------------------
    // 工作流定义
    // ----------------------------
    public class TestWorkflow : IWorkflow<TestData>
    {
        public string Id => "persist-test-workflow";
        public int Version => 1;

        public void Build(IWorkflowBuilder<TestData> builder)
        {
            builder
                .StartWith<InitStep>()
                    .Input(step => step.Data, data => data)

                .Then<ProcessStep>()
                    .Input(step => step.Data, data => data)

                .Then<CompleteStep>()
                    .Input(step => step.Data, data => data);
        }
    }
}
