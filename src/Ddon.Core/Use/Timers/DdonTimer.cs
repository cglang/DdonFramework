using System;
using System.Timers;

namespace Ddon.Core.Use.Timers
{
    /// <summary>
    /// 指定开始时间,间隔指定秒数执行事件
    /// </summary>
    public class DdonTimer : IDisposable
    {
        private readonly Timer timer;
        private DateTime baseTime;

        public ElapsedEventHandler? Elapsed { get; set; }

        public TimeSpan Interval { get; set; }

        public DdonTimer(TimeSpan interval)
        {
            baseTime = DateTime.UtcNow;

            Interval = interval;

            timer = new Timer() { Interval = 100 };
            timer.Elapsed += (sender, e) =>
            {
                if (Elapsed is not null && baseTime.AddMilliseconds(Interval.TotalMilliseconds) < DateTime.UtcNow)
                {
                    Elapsed.Invoke(sender, e);
                    baseTime = baseTime.AddMilliseconds(Interval.TotalMilliseconds);
                }
            };
        }

        public void Start()
        {
            if (Elapsed is null)
            {
                throw new NullReferenceException("Elapsed Is Null");
            }

            timer.Enabled = true;
            timer.Start();
        }

        public void Stop()
        {
            timer.Enabled = false;
            timer.Stop();
        }

        #region Dispose

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    timer.Dispose();
                }

                disposed = true;
            }
        }

        ~DdonTimer()
        {
            Dispose(disposing: false);
        }

        #endregion
    }
}