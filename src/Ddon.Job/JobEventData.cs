using System;

namespace Ddon.Job
{
    public class JobEventData : IJobEventData
    {
        public JobEventData(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
