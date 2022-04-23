using System;
using System.Timers;

namespace Ddon.Core.System.Timers
{
    class DdonTimer : Timer
    {
        private const double DefaultInterval = 60000;

        private bool isDate = false;

        private bool isRepeat = false;

        private DateTime? date;

        private double repeatInterval;

        public DdonTimer SetAction(Action action)
        {
            Elapsed += (_, _) =>
            {
                var now = DateTime.Now;
                if (isDate)
                {
                    if (now >= date)
                    {
                        if (isRepeat)
                        {
                            var date_s = date.Value.AddMilliseconds(repeatInterval);
                            if (now >= date_s)
                            {
                                date = date_s;
                                action();
                            }
                        }
                        else
                        {
                            action();
                            Stop();
                        }
                    }
                }
                else
                {
                    action();
                }
            };

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

        public new void Start()
        {
            base.Start();
        }
    }
}