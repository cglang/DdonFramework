using System;
using System.Timers;

namespace Ddon.Core.System.Timers
{
    public class DdonTimer : Timer
    {
        public new Action? Elapsed { get; set; }

        private DateTime _baseTime;             // 基准时间

        private readonly DateTime? _startDate;           // 开始时间
        private readonly double _interval;               // 间隔时间

        /// <summary>
        /// DdonTimer
        /// </summary>
        /// <param name="startDate">开始时间 UTC Time</param>
        /// <param name="interval">间隔时间</param>
        public DdonTimer(DateTime? startDate, TimeSpan interval)
        {
            _baseTime = DateTime.Now;
            _startDate = startDate;
            _interval = interval.TotalMilliseconds;

            // 间隔时间大于一小时每分钟检查一次,否则每秒检查一次
            //Interval = _interval > 3600000 ? 60000 : 1000;
            Interval = 1000;

            base.Elapsed += (_, _) =>
            {
                if (Elapsed == null) return;

                var now = DateTime.Now;
                if (_startDate != null && now < _startDate) return;

                if (_baseTime.AddMilliseconds(_interval) < now)
                {
                    Elapsed();
                    _baseTime = now;
                }
            };
        }

        /// <summary>
        /// DdonTimer
        /// </summary>
        /// <param name="startDate">开始时间 UTC Time</param>
        /// <param name="interval">间隔时间</param>
        public DdonTimer(DateTime? startDate, double interval)
        {
            if (interval <= 0) throw new Exception("间隔时间不可小于0");

            _baseTime = DateTime.Now;
            _startDate = startDate;
            _interval = interval;

            // 间隔时间大于一小时每分钟检查一次,否则每秒检查一次
            //Interval = _interval > 3600000 ? 60000 : 1000;
            Interval = 1000;

            base.Elapsed += (_, _) =>
            {
                if (Elapsed == null) return;

                var now = DateTime.Now;
                if (_startDate != null && now < _startDate) return;

                if (_baseTime.AddMilliseconds(_interval) < now)
                {
                    Elapsed();
                    _baseTime = now;
                }
            };
        }
    }
}