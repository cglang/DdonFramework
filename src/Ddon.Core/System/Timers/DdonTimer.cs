using System;
using System.Timers;

namespace Ddon.Core.System.Timers
{
    public class DdonTimer : Timer
    {
        private const double DefaultInterval = 60000;

        private bool isDate = false;

        private bool isRepeat = false;

        private DateTime? date;

        private double repeatInterval;

        public DdonTimer InitAction(Action action)
        {
            Elapsed += (_, _) =>
            {
                var now = DateTime.Now;

                if (!isDate) { action(); }
                else if (now >= date)
                {
                    if (!isRepeat)
                    {
                        action();
                        Stop();
                    }
                    else if (now >= date.Value.AddMilliseconds(repeatInterval))
                    {
                        date = date.Value.AddMilliseconds(repeatInterval);
                        action();
                    }
                }
            };

            return this;
        }

        public DdonTimer FinishAction(Action p)
        {
            p();
            return this;
        }

        public DdonTimer SetInterval(TimeSpan interval)
        {
            Interval = interval.TotalMilliseconds;
            return this;
        }

        public DdonTimer SetDateTime(DateTime dateTime, TimeSpan? interval = null)
        {
            isDate = true;
            if (interval is null)
            {
                Interval = DefaultInterval;
            }
            else
            {
                Interval = DefaultInterval;
                repeatInterval = interval.Value.TotalMilliseconds;
                isRepeat = true;
            }
            date = dateTime;
            return this;
        }

        public DdonTimer SetRule(DdonTimerRule rule)
        {
            if (rule.DateTime != null)
            {
                SetDateTime(rule.DateTime.Value, rule.Interval);
            }
            else
            {
                SetInterval(rule.Interval!.Value);
            }
            return this;
        }

        public new void Start()
        {
            base.Start();
        }
    }

    public class DdonTimerRule
    {
        public DdonTimerRule(DateTime? dateTime, TimeSpan? interval)
        {
            if (dateTime == null && interval == null)
            {
                throw new ArgumentNullException($"{nameof(dateTime)} {nameof(interval)} 不可同时为空");
            }
            DateTime = dateTime;
            Interval = interval;
        }

        public DateTime? DateTime { get; set; }

        public TimeSpan? Interval { get; set; }
    }
}