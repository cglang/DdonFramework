using System;

namespace Ddon.Core.System.Timers
{
    public class DdonJobRule
    {
        public DdonJobRule(DateTime dateTime, DateTime? endTime, TimeSpan? interval)
        {
            DateTime = dateTime;
            EndTime = endTime;
            Interval = interval?.TotalMilliseconds;
        }

        public DdonJobRule(DateTime dateTime, DateTime? endTime, double? interval)
        {
            DateTime = dateTime.ToLocalTime();
            EndTime = endTime?.ToLocalTime();
            Interval = interval;
        }

        public DdonJobRule() { }

        /// <summary>
        /// Job触发事件的时间
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 两次事件间隔的事件(小时)
        /// </summary>
        public double? Interval { get; set; }
    }
}