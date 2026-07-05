using Ddon.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test.Hosting;

[TestClass]
public class PeriodicBackgroundTaskTest
{
    [TestMethod]
    public async Task RunImmediately_ExecutesOnStart()
    {
        var executed = new TaskCompletionSource();
        var task = CreateTestTask(
            onExecute: ct => { executed.TrySetResult(); return Task.CompletedTask; });

        await task.StartAsync(default);
        await executed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await task.StopAsync(default);
    }

    [TestMethod]
    public async Task NotRunImmediately_WaitsForPeriod()
    {
        var executed = new TaskCompletionSource();
        var task = CreateTestTask(
            onExecute: ct => { executed.TrySetResult(); return Task.CompletedTask; },
            configure: o => o.RunImmediately = false);

        var startTime = DateTime.UtcNow;
        await task.StartAsync(default);
        await executed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var elapsed = DateTime.UtcNow - startTime;

        Assert.IsTrue(elapsed >= task.PeriodValue - TimeSpan.FromMilliseconds(30));
        await task.StopAsync(default);
    }

    [TestMethod]
    public async Task RunsOnEveryPeriod()
    {
        var executionCount = 0;
        var tcs = new TaskCompletionSource();
        var task = CreateTestTask(
            onExecute: ct =>
            {
                if (Interlocked.Increment(ref executionCount) >= 3)
                    tcs.TrySetResult();
                return Task.CompletedTask;
            },
            configure: o =>
            {
                o.RunImmediately = true;
                o.Period = TimeSpan.FromMilliseconds(10);
            });

        await task.StartAsync(default);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsTrue(executionCount >= 3);
        await task.StopAsync(default);
    }

    [TestMethod]
    public async Task Exception_ContinuesExecution()
    {
        int count = 0;
        var secondExec = new TaskCompletionSource();
        var task = CreateTestTask(
            onExecute: ct =>
            {
                var c = Interlocked.Increment(ref count);
                if (c == 1) throw new InvalidOperationException("test error");
                secondExec.TrySetResult();
                return Task.CompletedTask;
            },
            configure: o => o.RunImmediately = true);

        await task.StartAsync(default);
        await secondExec.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.AreEqual(2, count);
        await task.StopAsync(default);
    }

    [TestMethod]
    public async Task Cancel_StopsGracefully()
    {
        var executed = new TaskCompletionSource();
        using var cts = new CancellationTokenSource();
        var task = CreateTestTask(
            onExecute: ct =>
            {
                executed.TrySetResult();
                return Task.CompletedTask;
            },
            configure: o => o.RunImmediately = true);

        await task.StartAsync(default);
        await executed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await cts.CancelAsync();
        await task.StopAsync(cts.Token);
    }

    [TestMethod]
    public async Task AllowConcurrentExecution_False_PreventsOverlap()
    {
        var execStarted = new TaskCompletionSource();
        var execBlock = new TaskCompletionSource();
        var execCount = 0;

        var task = CreateTestTask(
            onExecute: async ct =>
            {
                Interlocked.Increment(ref execCount);
                execStarted.TrySetResult();
                await execBlock.Task;
            },
            configure: o =>
            {
                o.RunImmediately = true;
                o.AllowConcurrentExecution = false;
                o.Period = TimeSpan.FromMilliseconds(10);
            });

        await task.StartAsync(default);
        await execStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Task.Delay(200);

        Assert.AreEqual(1, execCount);
        execBlock.TrySetResult();
        await task.StopAsync(default);
    }

    [TestMethod]
    public async Task AllowConcurrentExecution_True_DoesNotThrow()
    {
        var execBlock = new TaskCompletionSource();
        var execCount = 0;

        var task = CreateTestTask(
            onExecute: async ct =>
            {
                Interlocked.Increment(ref execCount);
                await execBlock.Task;
            },
            configure: o =>
            {
                o.RunImmediately = false;
                o.AllowConcurrentExecution = true;
                o.Period = TimeSpan.FromMilliseconds(10);
            });

        await task.StartAsync(default);
        await Task.Delay(100);

        execBlock.TrySetResult();
        await task.StopAsync(default);
    }

    [TestMethod]
    public async Task Stop_WithoutStart_DoesNotThrow()
    {
        var task = CreateTestTask();

        await task.StopAsync(default);
    }

    private static TestPeriodicTask CreateTestTask(
        Func<CancellationToken, Task>? onExecute = null,
        Action<TestPeriodicTaskOptions>? configure = null)
    {
        var opts = new TestPeriodicTaskOptions();
        configure?.Invoke(opts);

        var sp = new ServiceCollection().BuildServiceProvider();
        var logger = NullLogger.Instance;

        return new TestPeriodicTask(sp, logger, opts, onExecute ?? (_ => Task.CompletedTask));
    }
}

internal class TestPeriodicTaskOptions
{
    public TimeSpan Period { get; set; } = TimeSpan.FromMilliseconds(50);
    public bool RunImmediately { get; set; } = true;
    public bool AllowConcurrentExecution { get; set; } = false;
}

internal class TestPeriodicTask : PeriodicBackgroundTask
{
    private readonly TestPeriodicTaskOptions _options;
    private readonly Func<CancellationToken, Task> _onExecute;

    public TimeSpan PeriodValue => _options.Period;

    public TestPeriodicTask(
        IServiceProvider sp,
        ILogger logger,
        TestPeriodicTaskOptions options,
        Func<CancellationToken, Task> onExecute)
        : base(sp, logger)
    {
        _options = options;
        _onExecute = onExecute;
    }

    protected override TimeSpan Period => _options.Period;
    protected override bool RunImmediately => _options.RunImmediately;
    protected override bool AllowConcurrentExecution => _options.AllowConcurrentExecution;
    protected override Task OnExecuteAsync(CancellationToken cancellationToken)
        => _onExecute(cancellationToken);
}
