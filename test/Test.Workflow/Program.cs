using Ddon.Workflow;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Test.DemoWorkflow;

namespace Test.Workflow
{
    internal class Program
    {
        public static IServiceProvider ServiceProvider = null!;

        public static async Task Main(string[] args)
        {
            await TestProgram.Main(args);

            return;

            ServiceProvider = ConfigureServices();

            var logger = ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("=== Ddon.ICWorkflow 系统启动 ===");

            // 1. 初始化硬件模拟
            var stacker = new StackerSimulator();
            var line1Simulator = new LineSimulator("线体-1");
            var line2Simulator = new LineSimulator("线体-2");

            // 2. 初始化调度器
            var scheduler = ServiceProvider.GetRequiredService<IWorkflowScheduler>();

            // 3. 创建线体1的工作流任务
            var context1 = new DemoContext()
            {
                Stacker = stacker,
                Task = new OutboundTask { TaskNo = "T101", OriginalPosition = "Bank-A1", TargetPosition = "Line-1" },
                Line = line1Simulator,
            };

            var steps1 = new List<IStep<DemoContext>>
            {
                new StackerCommandStep(),
                new ActionStep<DemoContext>((_) =>
                {
                    logger.LogDebug("空步骤");
                    return Task.CompletedTask;
                }),
                new WaitStackerStep(),
                new ConfirmMaterialStep()
            };
            var wf1 = new Workflow<DemoContext>("线体出库任务", context1, steps1);

            // 4. 创建线体2的工作流任务 (模拟稍后到达的任务)
            var context2 = new DemoContext()
            {
                Stacker = stacker,
                Task = new OutboundTask { TaskNo = "T202", OriginalPosition = "Bank-B5", TargetPosition = "Line-2" },
                Line = line2Simulator,
            };

            var steps2 = new List<IStep<DemoContext>>
            {
                new StackerCommandStep(),
                new ActionStep<DemoContext>((_) =>
                {
                    logger.LogDebug("空步骤");
                    return Task.CompletedTask;
                }),
                new WaitStackerStep(),
                new ConfirmMaterialStep()
            };
            var wf2 = new Workflow<DemoContext>("线体出库任务", context2, steps2);

            // 注册到调度器
            await scheduler.StartAsync(wf1);
            await Task.Delay(1000); // 模拟一秒后第二个线体也开始工作
            await scheduler.StartAsync(wf2);

            // 5. 主循环轮询 (10Hz 刷新频率)
            logger.LogDebug(">>> 正在轮询处理硬件状态...");
            while (true)
            {
                await scheduler.UpdateAsync();
                await Task.Delay(100);
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddSimpleConsole(option =>
                {
                    option.SingleLine = true;
                });

                builder.SetMinimumLevel(LogLevel.Trace);
            });
            services.AddWorkflow();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }
    }

    public class CustomConsoleFormatter : ConsoleFormatter
    {
        private readonly CustomConsoleFormatterOptions _options;

        public CustomConsoleFormatter(IOptions<CustomConsoleFormatterOptions> options)
            : base(nameof(CustomConsoleFormatter))
        {
            _options = options.Value;
        }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
            if (string.IsNullOrEmpty(message) && logEntry.Exception == null)
            {
                return;
            }

            var timestamp = DateTime.Now.ToString(
                _options.TimestampFormat ?? "yyyy-MM-dd HH:mm:ss.fff");

            var level = GetShortLevel(logEntry.LogLevel);

            // 核心输出（无类名）
            textWriter.Write($"[{timestamp}] [{level}] {message}");

            // Scope（可选）
            if (_options.IncludeScopes && scopeProvider != null)
            {
                scopeProvider.ForEachScope((scope, writer) =>
                {
                    writer.Write($" => {scope}");
                }, textWriter);
            }

            textWriter.WriteLine();

            // 异常
            if (logEntry.Exception != null)
            {
                textWriter.WriteLine(logEntry.Exception);
            }
        }

        private static string GetShortLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => "TRC",
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "CRT",
                _ => "UNK"
            };
        }
    }

    public class CustomConsoleFormatterOptions : ConsoleFormatterOptions
    {
        public bool IncludeScopes { get; set; } = false;
    }
}
