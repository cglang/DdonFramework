using System;

namespace Ddon.Job
{
    public class Mission
    {
        public Mission(MissionRule rule, Action action)
        {
            Id = Guid.NewGuid();
            Rule = rule;
            Action = action;
        }

        public Guid Id { get; set; }

        public MissionState State { get; set; }

        public MissionRule Rule { get; set; }

        public Action Action { get; set; }

        public void Start()
        {
            State = MissionState.Started;
        }

        public void Stop()
        {
            State = MissionState.Stoped;
        }
    }
}
