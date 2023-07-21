using System;

namespace Ddon.Socket.Session
{
    internal class DdonSocketResponse
    {
        public Guid Id { get; }

        public bool IsCompleted { get; private set; }

        public Action<string> ActionThen;
        public Action<string> ExceptionThen;

        public DdonSocketResponse(Action<string> action, Action<string> exception)
        {
            Id = Guid.NewGuid();

            ActionThen = _ => IsCompleted = true;
            ExceptionThen = _ => IsCompleted = true;

            ActionThen += action;
            ExceptionThen += exception;
        }
    }
}
