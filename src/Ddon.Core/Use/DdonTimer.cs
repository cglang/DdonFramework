﻿using System;
using System.Timers;

namespace Ddon.Core.Use
{
    /// <summary>
    /// 指定开始时间,间隔指定秒数执行事件
    /// </summary>
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
            _baseTime = DateTime.UtcNow;
            _startDate = startDate?.Kind == DateTimeKind.Utc ? startDate : startDate?.ToUniversalTime();
            _interval = interval.TotalSeconds;

            Interval = 1000;

            InitElapsed();
        }

        private void InitElapsed()
        {
            base.Elapsed += (_, _) =>
            {
                if (Elapsed == null) return;
                var now = DateTime.UtcNow;
                if (_startDate != null && now < _startDate) return;
                if (_baseTime.AddSeconds(_interval) < now)
                {
                    Elapsed();
                    _baseTime = _baseTime.AddSeconds(_interval);
                }
            };
        }
    }
}
