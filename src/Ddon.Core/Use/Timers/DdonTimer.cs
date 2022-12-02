using System;
using System.Timers;

namespace Ddon.Core.Use.Timers
{
    /// <summary>
    /// 指定开始时间,间隔指定秒数执行事件
    /// </summary>
    public class DdonTimer : IDisposable
    {
        private readonly Timer _timer;

        private static ElapsedEventHandler? Elapsed => null;

        private TimeSpan Interval { get; set; }

        public DdonTimer(TimeSpan interval)
        {
            var baseTime = DateTime.UtcNow;

            Interval = interval;

            _timer = new Timer() { Interval = 100 };
            _timer.Elapsed += (sender, e) =>
            {
                if (Elapsed is null || baseTime.AddMilliseconds(Interval.TotalMilliseconds) >= DateTime.UtcNow) return;

                Elapsed.Invoke(sender, e);
                baseTime = baseTime.AddMilliseconds(Interval.TotalMilliseconds);
            };
        }

        public void Start()
        {
            if (Elapsed is null)
            {
                throw new NullReferenceException("Elapsed Is Null");
            }

            _timer.Enabled = true;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Enabled = false;
            _timer.Stop();
        }

        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                _disposed = true;
            }
        }

        ~DdonTimer()
        {
            Dispose(disposing: false);
        }

        #endregion
    }
}