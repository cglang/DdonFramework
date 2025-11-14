using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.Utilities.WheelTimer
{
    public sealed partial class HashedWheelTimer : IDisposable
    {
        private readonly long _tickDuration;        // tick 时长 (ns)
        private readonly int _wheelSize;            // 槽数
        private readonly HashedWheelBucket[] _wheel;
        private readonly Thread _workerThread;
        private readonly ITimeoutExecutor _executor;

        private readonly BlockingCollection<HashedWheelTimeout> _newTimeouts = new BlockingCollection<HashedWheelTimeout>();

        private volatile bool _running = true;
        private long _startTime;                    // ns
        private long _tick = 0;                     // 当前 tick

        public HashedWheelTimer(TimeSpan tickDuration, int ticksPerWheel = 512, ITimeoutExecutor executor = null)
        {
            if ((ticksPerWheel & ticksPerWheel - 1) != 0)
                throw new ArgumentException("wheelSize must be a power of 2");

            _wheelSize = NormalizeTicksPerWheel(ticksPerWheel);
            _tickDuration = tickDuration.Ticks * 100; // ticks -> ns
            _executor = executor ?? new DefaultExecutor();

            _wheel = new HashedWheelBucket[_wheelSize];
            for (var i = 0; i < _wheelSize; i++)
                _wheel[i] = new HashedWheelBucket(_executor);

            _workerThread = new Thread(Worker)
            {
                IsBackground = true
            };
            _workerThread.Start();
        }

        private static int NormalizeTicksPerWheel(int ticksPerWheel)
        {
            var normalized = 1;
            while (normalized < ticksPerWheel)
                normalized <<= 1;
            return normalized;
        }

        public ITimeout NewTimeout(Action callback, TimeSpan delay)
        {
            var timeout = new HashedWheelTimeout(callback, delay);
            _newTimeouts.Add(timeout);
            return timeout;
        }

        public ITimeout NewTimeout(Func<Task> callbackAsync, TimeSpan delay)
        {
            var timeout = new HashedWheelTimeout(callbackAsync, delay);
            _newTimeouts.Add(timeout);
            return timeout;
        }

        private void Worker()
        {
            _startTime = Now();
            while (_running)
            {
                var deadline = WaitForNextTick();
                if (deadline <= 0) continue;

                var index = (int)(_tick & _wheelSize - 1);
                var bucket = _wheel[index];

                bucket.ExpireTimeouts();

                DrainNewTimeouts();

                _tick++;
            }
        }

        private void DrainNewTimeouts()
        {
            while (_newTimeouts.TryTake(out var timeout))
            {
                long delayNs = timeout.Delay.Ticks * 100;
                var calculated = delayNs / _tickDuration;

                var execTick = _tick + calculated;
                var stopIndex = (int)(execTick & _wheelSize - 1);

                timeout.RemainingRounds = (long)Math.Floor((double)calculated / _wheelSize);
                _wheel[stopIndex].Add(timeout);
            }
        }

        private long WaitForNextTick()
        {
            var deadline = (_tick + 1) * _tickDuration;
            for (; ; )
            {
                var current = Now() - _startTime;
                var sleepNs = deadline - current;
                if (sleepNs <= 0) return current;

                var sleepMs = (int)(sleepNs / 100_0000);
                if (sleepMs <= 0) sleepMs = 1;
                Thread.Sleep(sleepMs);
            }
        }

        private static long Now() => DateTime.UtcNow.Ticks * 100;

        public void Dispose()
        {
            _running = false;
            _workerThread.Join();
        }
    }
}
