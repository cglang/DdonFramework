using System;

namespace Ddon.Socket.Session
{
    internal class DdonSocketResponseHandler
    {
        public Guid Id { get; set; }

        public DateTime Time { get; set; }

        public Action<string>? ActionThen;
        public Action<string>? ExceptionThen;

        public DdonSocketResponseHandler(Guid id)
        {
            Id = id;
            Time = DateTime.Now;
        }

        public DdonSocketResponseHandler Then(Action<string> action)
        {
            ActionThen = action;
            DdonSocketResponsePool.GetInstance().Pairs.Add(Id, this);
            return this;
        }

        public DdonSocketResponseHandler Exception(Action<string> action)
        {
            ExceptionThen = action;
            return this;
        }
    }
}
