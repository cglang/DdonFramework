using System;

namespace Ddon.Core.System.Timers
{
    public class DdonJobRule
    {
        public DdonJobRule(DateTime dateTime, TimeSpan? interval)
        {
            DateTime = dateTime;
            Interval = interval?.TotalMilliseconds;
        }

        public DdonJobRule(DateTime dateTime, double? interval)
        {
            DateTime = dateTime;
            Interval = interval;
        }

        public DdonJobRule() { }

        public DateTime DateTime { get; set; }

        public double? Interval { get; set; }
    }
}