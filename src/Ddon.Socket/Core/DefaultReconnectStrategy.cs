using System;
using Ddon.Socket.Abstractions;

namespace Ddon.Socket.Core
{
    public class DefaultReconnectStrategy : IReconnectStrategy
    {
        private static readonly TimeSpan[] DefaultDelays = new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(8),
            TimeSpan.FromSeconds(15),
        };

        private readonly TimeSpan[] _delays;
        private readonly TimeSpan _maxDelay;

        public DefaultReconnectStrategy()
            : this(DefaultDelays, TimeSpan.FromSeconds(30))
        {
        }

        public DefaultReconnectStrategy(TimeSpan[] delays, TimeSpan maxDelay)
        {
            _delays = delays ?? DefaultDelays;
            _maxDelay = maxDelay;
        }

        public TimeSpan GetNextDelay(int retryCount)
        {
            if (retryCount <= 0) return TimeSpan.Zero;

            var delayIndex = Math.Min(retryCount - 1, _delays.Length - 1);
            var delay = _delays[delayIndex];

            if (delay > _maxDelay)
                delay = _maxDelay;

            return delay;
        }

        public void Reset()
        {
        }
    }
}
