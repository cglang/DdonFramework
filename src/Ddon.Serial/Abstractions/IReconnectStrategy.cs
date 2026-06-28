using System;

namespace Ddon.Serial.Abstractions
{
    public interface IReconnectStrategy
    {
        TimeSpan GetNextDelay(int retryCount);

        void Reset();
    }
}
