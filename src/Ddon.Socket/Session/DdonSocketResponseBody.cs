using System;

namespace Ddon.Socket.Session
{
    internal class DdonSocketResponseBody
    {
        public Guid Id { get; set; }

        public DateTime Time { get; set; }

        public Action<string>? ActionThen;
        public Action<string>? ExceptionThen;

        public DdonSocketResponseBody(Guid id)
        {
            Id = id;
            Time = DateTime.Now;
        }

        public DdonSocketResponseBody Then(Action<string> action)
        {
            ActionThen = action;
            DdonSocketResponsePool.GetInstance().Pairs.Add(Id, this);
            return this;
        }

        public DdonSocketResponseBody Exception(Action<string> action)
        {
            ExceptionThen = action;
            return this;
        }
    }
}
