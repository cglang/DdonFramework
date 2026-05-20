using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.Utilities.MainLoop
{
    /// <summary>
    /// 同步主循环：循环体为 <c>Action&lt;CancellationToken&gt;</c>，
    /// 内部通过 <see cref="Task.Run"/> 在线程池执行，不阻塞调用线程。
    /// </summary>
    /// <example>
    /// <code>
    /// // WinForms：在 UI 线程构造，事件自动回 UI 线程，无需 Invoke
    /// var loop = new SyncMainLoop(
    ///     interval: TimeSpan.FromMilliseconds(500),
    ///     loopAction: ct => ProcessBatch(ct));
    ///
    /// loop.AfterLoop += (_, args) =>
    ///     label1.Text = $"第 {args.ExecutionCount} 次";  // 直接访问控件，安全
    ///
    /// loop.Start();
    /// </code>
    /// </example>
    public sealed class SyncMainLoop : MainLoopBase
    {
        private readonly Action<CancellationToken> _loopAction;

        /// <param name="interval">循环间隔</param>
        /// <param name="loopAction">每次循环执行的同步委托</param>
        /// <param name="highPrecision">高精度模式：用间隔减去执行耗时作为实际等待时间</param>
        /// <param name="captureUiContext">
        /// 是否捕获 UI 同步上下文（默认 true）。
        /// 在 UI 线程上构造时保持默认值，事件回调自动回 UI 线程。
        /// </param>
        public SyncMainLoop(
            TimeSpan interval,
            Action<CancellationToken> loopAction,
            bool highPrecision = false,
            bool captureUiContext = true)
            : base(interval, highPrecision, captureUiContext)
        {
            _loopAction = loopAction ?? throw new ArgumentNullException(nameof(loopAction));
        }

        protected override Task ExecuteOnceAsync(CancellationToken cancellationToken)
            => Task.Run(() => _loopAction(cancellationToken), cancellationToken);
    }
}
