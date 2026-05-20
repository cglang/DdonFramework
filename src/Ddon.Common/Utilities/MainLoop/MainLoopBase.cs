using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.Utilities.MainLoop
{
    /// <summary>
    /// 主循环公共基类，封装生命周期、统计、事件等通用逻辑。
    /// <para>
    /// 若在 UI 线程（WinForms / WPF）上构造，所有事件回调会自动
    /// 通过 <see cref="SynchronizationContext"/> 切回 UI 线程触发，
    /// 无需在事件处理器里手动 Invoke。
    /// </para>
    /// </summary>
    public abstract class MainLoopBase : IDisposable
    {
        // ── 内部状态 ──────────────────────────────────────────────
        private readonly object _syncLock = new object();
        private CancellationTokenSource _cts;
        private Task _executionTask;
        private bool _isRunning;
        private bool _disposed;

        private long _executionCount;
        private long _exceptionCount;

        // ── 配置 ──────────────────────────────────────────────────
        protected readonly TimeSpan Interval;
        protected readonly bool HighPrecision;

        // ── UI 线程同步上下文（可为 null，表示无需 marshal） ────────
        private readonly SynchronizationContext _uiContext;

        // ── 计时器（每次循环共享） ─────────────────────────────────
        private readonly Stopwatch _cycleStopwatch = new Stopwatch();

        // ────────────────────────────────────────────────────────────
        //  事件
        // ────────────────────────────────────────────────────────────

        /// <summary>循环已启动</summary>
        public event EventHandler Started;

        /// <summary>循环已停止</summary>
        public event EventHandler Stopped;

        /// <summary>每次执行前触发</summary>
        public event EventHandler<LoopEventArgs> BeforeLoop;

        /// <summary>每次执行后触发</summary>
        public event EventHandler<LoopEventArgs> AfterLoop;

        /// <summary>循环内发生异常时触发</summary>
        public event EventHandler<Exception> LoopException;

        // ────────────────────────────────────────────────────────────
        //  属性
        // ────────────────────────────────────────────────────────────

        /// <summary>循环是否正在运行</summary>
        public bool IsRunning
        {
            get
            {
                lock (_syncLock)
                {
                    return _isRunning
                           && _executionTask != null
                           && !_executionTask.IsCompleted;
                }
            }
        }

        /// <summary>总执行次数</summary>
        public long TotalExecutionCount => Interlocked.Read(ref _executionCount);

        /// <summary>总异常次数</summary>
        public long TotalExceptionCount => Interlocked.Read(ref _exceptionCount);

        /// <summary>
        /// 是否捕获到了 UI 同步上下文。
        /// true 时所有事件回调自动回 UI 线程触发。
        /// </summary>
        public bool HasUiContext => _uiContext != null;

        // ────────────────────────────────────────────────────────────
        //  构造
        // ────────────────────────────────────────────────────────────

        /// <param name="interval">循环间隔，必须大于零</param>
        /// <param name="highPrecision">高精度模式：用间隔减去执行耗时作为实际等待时间</param>
        /// <param name="captureUiContext">
        /// 是否捕获当前线程的 <see cref="SynchronizationContext"/>（默认 true）。
        /// 在 UI 线程上构造时设为 true，所有事件会自动回 UI 线程；
        /// 在非 UI 线程或控制台程序中设为 false 可跳过无意义的 Post 开销。
        /// </param>
        protected MainLoopBase(TimeSpan interval, bool highPrecision, bool captureUiContext)
        {
            if (interval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(interval), "间隔必须大于零");

            Interval = interval;
            HighPrecision = highPrecision;

            // 只有当前线程确实存在非默认上下文时才保存
            // SynchronizationContext.Current 在 UI 线程上是 WindowsFormsSynchronizationContext 等
            // 在线程池 / 控制台线程上是 null，无需保存
            if (captureUiContext)
            {
                _uiContext = SynchronizationContext.Current;
            }
        }

        // ────────────────────────────────────────────────────────────
        //  启动 / 停止
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// 启动主循环。已在运行时返回 false。
        /// </summary>
        public bool Start()
        {
            lock (_syncLock)
            {
                if (_isRunning) return false;

                _cts = new CancellationTokenSource();
                _isRunning = true;
                _executionTask = Task.Run(() => RunLoopAsync(_cts.Token));
            }

            RaiseEvent(() => Started?.Invoke(this, EventArgs.Empty));
            return true;
        }

        /// <summary>
        /// 停止主循环，等待至多 <paramref name="timeout"/> 时间。
        /// </summary>
        public async Task<bool> StopAsync(TimeSpan timeout = default)
        {
            CancellationTokenSource cts;
            Task task;

            lock (_syncLock)
            {
                if (!_isRunning || _cts == null) return false;

                _isRunning = false;
                cts = _cts;
                task = _executionTask;
            }

            cts.Cancel();

            try
            {
                if (task != null && !task.IsCompleted)
                {
                    var effectiveTimeout = timeout == default ? TimeSpan.FromSeconds(5) : timeout;
                    var completed = await Task
                        .WhenAny(task, Task.Delay(effectiveTimeout))
                        .ConfigureAwait(false);

                    if (completed != task)
                        return false; // 超时
                }

                RaiseEvent(() => Stopped?.Invoke(this, EventArgs.Empty));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>重启循环</summary>
        public async Task RestartAsync(TimeSpan stopTimeout = default)
        {
            await StopAsync(stopTimeout).ConfigureAwait(false);
            Start();
        }

        /// <summary>等待循环自然结束（通常用于测试或 hosted service）</summary>
        public Task WaitForCompletionAsync()
            => _executionTask ?? Task.CompletedTask;

        // ────────────────────────────────────────────────────────────
        //  统计
        // ────────────────────────────────────────────────────────────

        public LoopStatistics GetStatistics() => new LoopStatistics
        {
            IsRunning = IsRunning,
            TotalExecutions = TotalExecutionCount,
            TotalExceptions = TotalExceptionCount,
            Interval = Interval,
        };

        // ────────────────────────────────────────────────────────────
        //  子类实现
        // ────────────────────────────────────────────────────────────

        /// <summary>子类提供单次循环逻辑</summary>
        protected abstract Task ExecuteOnceAsync(CancellationToken cancellationToken);

        // ────────────────────────────────────────────────────────────
        //  核心循环
        // ────────────────────────────────────────────────────────────

        private async Task RunLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var eventArgs = new LoopEventArgs
                {
                    ExecutionCount = Interlocked.Read(ref _executionCount) + 1,
                    CancellationToken = cancellationToken,
                };

                try
                {
                    RaiseEvent(() => BeforeLoop?.Invoke(this, eventArgs));

                    _cycleStopwatch.Restart();
                    await ExecuteOnceAsync(cancellationToken).ConfigureAwait(false);
                    _cycleStopwatch.Stop();

                    Interlocked.Increment(ref _executionCount);

                    eventArgs.ElapsedTime = _cycleStopwatch.Elapsed;
                    RaiseEvent(() => AfterLoop?.Invoke(this, eventArgs));

                    await WaitIntervalAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _exceptionCount);
                    RaiseEvent(() => LoopException?.Invoke(this, ex));

                    // 异常后退避，最多 1 秒，防止异常风暴
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var backoff = (int)Math.Min(Interval.TotalMilliseconds * 2, 1000);
                        try
                        {
                            await Task.Delay(backoff, cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private async Task WaitIntervalAsync(CancellationToken cancellationToken)
        {
            TimeSpan delay = HighPrecision
                ? Interval - _cycleStopwatch.Elapsed
                : Interval;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        // ────────────────────────────────────────────────────────────
        //  事件派发：自动切回 UI 线程
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// 触发事件。若构造时捕获了 UI 上下文，则通过 Post 回 UI 线程触发（异步、不阻塞循环）；
        /// 否则直接在当前线程触发。
        /// </summary>
        private void RaiseEvent(Action action)
        {
            if (_uiContext != null)
            {
                // Post 是异步派发，不会阻塞循环线程
                _uiContext.Post(_ => action(), null);
            }
            else
            {
                action();
            }
        }

        // ────────────────────────────────────────────────────────────
        //  IDisposable
        // ────────────────────────────────────────────────────────────

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                // 只发取消信号，不阻塞等待，避免 UI 线程死锁
                // 若需要确保循环停止后再销毁，请先 await StopAsync()
                _cts?.Cancel();
                _cts?.Dispose();
            }
        }

        // ────────────────────────────────────────────────────────────
        //  嵌套类型
        // ────────────────────────────────────────────────────────────

        public class LoopEventArgs : EventArgs
        {
            /// <summary>本次循环编号（从 1 开始）</summary>
            public long ExecutionCount { get; internal set; }

            /// <summary>本次循环耗时（仅 AfterLoop 时有效）</summary>
            public TimeSpan ElapsedTime { get; internal set; }

            /// <summary>取消令牌</summary>
            public CancellationToken CancellationToken { get; internal set; }
        }

        public class LoopStatistics
        {
            public bool IsRunning { get; internal set; }
            public long TotalExecutions { get; internal set; }
            public long TotalExceptions { get; internal set; }
            public TimeSpan Interval { get; internal set; }
        }
    }
}
