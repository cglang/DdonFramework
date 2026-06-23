using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    /// <summary>
    /// 投递到任意 SynchronizationContext（UI 线程、自定义消息循环等）
    /// </summary>
    public sealed class SyncContextScheduler : IEventScheduler
    {
        private readonly SynchronizationContext _ctx;

        /// <param name="ctx">
        ///   传 null 时退化为 ThreadPool。
        ///   在 UI 线程上调用 SynchronizationContext.Current 即可拿到 UI 上下文。
        /// </param>
        public SyncContextScheduler(SynchronizationContext ctx)
            => _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));

        /// <summary>捕获当前线程的 SynchronizationContext（须在目标线程上调用）</summary>
        public static SyncContextScheduler Capture()
        {
            var msg = "当前线程没有 SynchronizationContext，请在 UI 线程或已安装上下文的线程上调用。";
            var ctx = SynchronizationContext.Current
                      ?? throw new InvalidOperationException(msg);
            return new SyncContextScheduler(ctx);
        }

        /// <summary>异步投递（不阻塞发布者）</summary>
        public void Schedule(Action action) => _ctx.Post(_ => action(), null);

        /// <summary>同步投递（阻塞发布者直到回调完成，慎用）</summary>
        public void ScheduleSync(Action action) => _ctx.Send(_ => action(), null);

        public Task ScheduleAsync(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            _ctx.Post(async _ =>
            {
                try
                {
                    await action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
        }
    }
}
