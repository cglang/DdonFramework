using Ddon.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test.Workflow;

namespace Test.DemoWorkflow
{
    public interface IDevice
    {
        bool IsTaskCompleted();
        void SendCommand(string from, string to);
    }

    public interface ILine
    {
        bool IsMaterialAtPickupPoint();
        void WriteOutboundCompleted(short value);
    }

    // 堆垛机模拟器
    public class StackerSimulator : IDevice
    {
        private readonly ILogger logger = Program.ServiceProvider.GetRequiredService<ILogger<Program>>();

        private DateTime? _startTime;

        public bool IsWorking { get; set; }

        private string _currentTask = string.Empty;

        public void SendCommand(string from, string to)
        {
            _currentTask = $"{from}->{to}";
            logger.LogDebug($"[PLC] 堆垛机动作开始: {_currentTask}");
            _startTime = DateTime.Now;
            IsWorking = true;
        }

        public bool IsTaskCompleted()
        {
            // 模拟堆垛机物理运行需要 2.5 秒
            if ((DateTime.Now - _startTime!.Value).TotalSeconds > 2.5)
            {
                logger.LogDebug($"[PLC] 堆垛机动作完成: {_currentTask}");
                IsWorking = false;
                return true;
            }

            return false;
        }
    }

    // 线体模拟器
    public class LineSimulator : ILine
    {
        private readonly ILogger logger = Program.ServiceProvider.GetRequiredService<ILogger<Program>>();

        public string LineId { get; }
        private bool _receivedSignal;
        private DateTime? _signalTime;

        public LineSimulator(string id) => LineId = id;

        public void WriteOutboundCompleted(short value)
        {
            logger.LogDebug($"[PLC] {LineId} 收到接货信号: {value}");
            _receivedSignal = true;
            _signalTime = DateTime.Now;
        }

        public bool IsMaterialAtPickupPoint()
        {
            if (!_receivedSignal) return false;
            // 模拟料箱在线体上滚动 1.5 秒到达传感器位
            return (DateTime.Now - _signalTime!.Value).TotalSeconds > 1.5;
        }
    }

    public class OutboundTask
    {
        public string TaskNo { get; set; } = string.Empty;

        public string OriginalPosition { get; set; } = string.Empty;

        public string TargetPosition { get; set; } = string.Empty;

        public int CurrentStep { get; set; } // 用于模拟数据库状态
    }

    // 步骤1：发送指令
    public class StackerCommandStep : Step<DemoContext>
    {
        private readonly ILogger logger = Program.ServiceProvider.GetRequiredService<ILogger<Program>>();

        public override Task<StepStatus> OnUpdateAsync(DemoContext context, CancellationToken cancellationToken)
        {
            lock (context.Stacker)
            {
                if (!context.Stacker.IsWorking)
                {
                    logger.LogDebug("向堆垛机发送移动命令");
                    context.Stacker.SendCommand(context.Task.OriginalPosition, context.Task.TargetPosition);
                    return Task.FromResult(StepStatus.Success);
                }
            }
            return Task.FromResult(StepStatus.Running);
        }

        public override Task OnExitAsync(DemoContext context, CancellationToken cancellationToken)
        {
            context.Task.CurrentStep = 2;
            return Task.CompletedTask;
        }
    }

    // 步骤2：等待堆垛机物理到位
    public class WaitStackerStep : Step<DemoContext>
    {
        private readonly ILogger logger = Program.ServiceProvider.GetRequiredService<ILogger<Program>>();

        public override Task<StepStatus> OnUpdateAsync(DemoContext context, CancellationToken cancellationToken)
        {
            if (context.Stacker.IsTaskCompleted())
            {
                logger.LogDebug("堆垛机向线体出库料箱完成");
                context.Line.WriteOutboundCompleted(1);
                return Task.FromResult(StepStatus.Success);
            }
            return Task.FromResult(StepStatus.Running);
        }

        public override Task OnExitAsync(DemoContext context, CancellationToken cancellationToken)
        {
            context.Task.CurrentStep = 3;
            return Task.CompletedTask;
        }
    }

    // 步骤3：确认传感器触发
    public class ConfirmMaterialStep : Step<DemoContext>
    {
        private readonly ILogger logger = Program.ServiceProvider.GetRequiredService<ILogger<Program>>();

        public override Task<StepStatus> OnUpdateAsync(DemoContext context, CancellationToken cancellationToken)
        {
            if (context.Line.IsMaterialAtPickupPoint())
            {
                logger.LogDebug($"[业务] 线体 {context.Task.TaskNo} 料箱到位确认成功!");
                return Task.FromResult(StepStatus.Success);
            }
            return Task.FromResult(StepStatus.Running);
        }

        public override Task OnExitAsync(DemoContext context, CancellationToken cancellationToken)
        {
            context.Task.CurrentStep = 6;
            return Task.CompletedTask;
        }
    }

    // 线体开始称量

    // 线体称量完毕进行回库

    public class DemoContext
    {
        public StackerSimulator Stacker { get; set; } = null!;

        public OutboundTask Task { get; set; } = null!;

        public LineSimulator Line { get; set; } = null!;
    }
}
