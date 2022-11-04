using System;

namespace Ddon.Job
{
    public interface IJobElement
    {
        public MissionRule Rule { get; }

        public Action Action { get; }
    }
}
