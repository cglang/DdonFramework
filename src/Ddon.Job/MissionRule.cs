using System;

namespace Ddon.Job
{
    public class MissionRule
    {
        public MissionRule() { }

        public MissionRule(DateTime startDate, TimeSpan interval)
        {
            StartDate = startDate;
            Interval = interval;
        }

        public MissionRule(DateTime startDate, DateTime? endTime, TimeSpan interval)
        {
            StartDate = startDate;
            EndTime = endTime;
            Interval = interval;
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 间隔时间
        /// </summary>
        public TimeSpan Interval { get; set; }
    }
}