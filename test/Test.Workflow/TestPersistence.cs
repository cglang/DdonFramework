using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Workflow;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Test.Workflow
{
    /// <summary>
    /// Ddon.Workflow 持久化与恢复测试
    /// 场景：工作流推进到第 2 步后"崩溃"，从检查点恢复并继续执行到完成
    /// </summary>
    internal class TestPersistence
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Ddon.Workflow 持久化/恢复测试 ===\n");

            var storagePath = Path.Combine(Path.GetTempPath(), "TestWorkflow_Checkpoints");
            if (Directory.Exists(storagePath)) Directory.Delete(storagePath, true);

            // ---- 第一次"运行"：启用持久化并推进到中途后崩溃 ----
            Console.WriteLine("[阶段1] 创建带持久化的工作流并运行...\n");

            var ctx1 = new PersistContext();
            var strategy1 = new FileSystemPersistenceStrategy(storagePath, NullLogger<FileSystemPersistenceStrategy>.Instance);

            var persistable = new PersistableWorkflow<PersistContext>(
                "持久化测试任务",
                ctx1,
                BuildSteps(),
                strategy1);

            var scheduler1 = new WorkflowScheduler(NullLogger<IWorkflowScheduler>.Instance);
            await scheduler1.StartAsync(persistable);

            // 更新1：步骤A完成 -> 步骤B(等待)进入
            await scheduler1.UpdateAsync();
            ctx1.Go = true; // 外部条件到达，步骤B可以完成

            // 更新2：步骤B完成 -> 检查点(index=2)保存，步骤C(等待)进入
            await scheduler1.UpdateAsync();

            // 此刻检查点应已保存 index=2
            var checkpointExists = await strategy1.CheckpointExistsAsync(persistable.Id);
            var checkpoint = await strategy1.LoadCheckpointAsync(persistable.Id);

            Assert(checkpointExists, "推进后应存在检查点");
            Assert(checkpoint != null && checkpoint.CurrentStepIndex == 2,
                $"检查点步骤索引应为 2，实际: {checkpoint?.CurrentStepIndex}");
            Console.WriteLine($"  检查点已保存: 当前步骤={checkpoint?.CurrentStepIndex}\n");

            Console.WriteLine("[阶段2] 模拟应用崩溃，丢弃当前工作流...\n");

            // ---- 第二次"运行"：从检查点恢复并继续执行到完成 ----
            Console.WriteLine("[阶段3] 从检查点恢复工作流...\n");

            var recoveryService = new WorkflowRecoveryService(strategy1, NullLogger<WorkflowRecoveryService>.Instance);
            var checkpoints = await recoveryService.GetRecoverableCheckpointsAsync();
            Assert(checkpoints.Length == 1, $"应恰好有 1 个检查点，实际: {checkpoints.Length}");

            var recovered = await recoveryService.RecoverWorkflowAsync<PersistContext>(
                checkpoints[0],
                (stepTypeNames, context) => RebuildSteps(stepTypeNames, context));

            Assert(recovered is PersistableWorkflow<PersistContext>, "恢复的工作流应仍支持持久化");

            // 模拟外部条件：恢复后让步骤C的等待条件满足
            recovered.Context.Finalize = true;

            var scheduler2 = new WorkflowScheduler(NullLogger<IWorkflowScheduler>.Instance);
            await scheduler2.StartAsync(recovered);

            // 继续驱动直到完成
            while (!recovered.IsFinished)
            {
                await scheduler2.UpdateAsync();
            }

            var finalCheckpointExists = await strategy1.CheckpointExistsAsync(recovered.Id);
            Assert(!finalCheckpointExists, "工作流完成后应清除检查点");
            Assert(recovered.Context.CurrentStep == 3, $"完成时 CurrentStep 应为 3，实际: {recovered.Context.CurrentStep}");

            Console.WriteLine($"\n[成功] 工作流 '{recovered.Name}' 恢复后完成，最终步骤: {recovered.Context.CurrentStep}\n");

            Directory.Delete(storagePath, true);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Console.WriteLine($"[失败] {message}");
                throw new InvalidOperationException(message);
            }
            Console.WriteLine($"[通过] {message}");
        }

        private sealed class NullLogger<T> : ILogger<T>
        {
            public static readonly NullLogger<T> Instance = new NullLogger<T>();
            public IDisposable? BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }

        private static IList<IStep<PersistContext>> BuildSteps()
        {
            return new List<IStep<PersistContext>>
            {
                new StepA(),
                new StepB(),
                new StepC()
            };
        }

        private static IStep<PersistContext>[] RebuildSteps(string[] stepTypeNames, PersistContext context)
        {
            var steps = new List<IStep<PersistContext>>();
            foreach (var typeName in stepTypeNames)
            {
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    var step = Activator.CreateInstance(type) as IStep<PersistContext>;
                    if (step != null)
                        steps.Add(step);
                }
            }
            return steps.ToArray();
        }

        public class PersistContext
        {
            public int CurrentStep { get; set; }
            public bool Go { get; set; }
            public bool Finalize { get; set; }
        }

        public class StepA : Step<PersistContext>
        {
            public override Task<StepStatus> OnUpdateAsync(PersistContext context, CancellationToken cancellationToken)
                => Task.FromResult(StepStatus.Success);

            public override Task OnExitAsync(PersistContext context, CancellationToken cancellationToken)
            {
                context.CurrentStep = 1;
                return Task.CompletedTask;
            }
        }

        public class StepB : Step<PersistContext>
        {
            public override Task<StepStatus> OnUpdateAsync(PersistContext context, CancellationToken cancellationToken)
                => Task.FromResult(context.Go ? StepStatus.Success : StepStatus.Running);

            public override Task OnExitAsync(PersistContext context, CancellationToken cancellationToken)
            {
                context.CurrentStep = 2;
                return Task.CompletedTask;
            }
        }

        public class StepC : Step<PersistContext>
        {
            public override Task<StepStatus> OnUpdateAsync(PersistContext context, CancellationToken cancellationToken)
                => Task.FromResult(context.Finalize ? StepStatus.Success : StepStatus.Running);

            public override Task OnExitAsync(PersistContext context, CancellationToken cancellationToken)
            {
                context.CurrentStep = 3;
                return Task.CompletedTask;
            }
        }
    }
}
