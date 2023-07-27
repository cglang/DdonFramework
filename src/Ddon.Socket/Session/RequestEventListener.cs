using System;

namespace Ddon.Socket.Session
{
    internal class RequestEventListener
    {
        public Guid Id { get; }

        public bool IsCompleted { get; private set; }

        public Action<string> ActionThen;
        public Action<Exception> ExceptionThen;

        public RequestEventListener(Action<string> action, Action<Exception> exception)
        {
            Id = Guid.NewGuid();

            ActionThen = _ => IsCompleted = true;
            ExceptionThen = _ => IsCompleted = true;

            ActionThen += action;
            ExceptionThen += exception;
        }
    }
}
