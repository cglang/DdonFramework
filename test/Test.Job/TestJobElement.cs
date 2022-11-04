using Ddon.Job;
using System;

namespace Test.Job
{
    internal class TestJobElement : IJobElement
    {
        public MissionRule Rule => new(DateTime.Now, new TimeSpan(0, 0, 0, 1));

        public Action Action => () =>
        {
            Console.WriteLine("哈哈哈");
        };
    }
}
