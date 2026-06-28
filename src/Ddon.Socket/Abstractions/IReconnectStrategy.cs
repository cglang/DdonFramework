using System;

namespace Ddon.Socket.Abstractions
{
    public interface IReconnectStrategy
    {
        TimeSpan GetNextDelay(int retryCount);

        void Reset();
    }
}
