using Ddon.Job;
using Microsoft.Extensions.Logging;

namespace Test.Job
{
    internal class TestJob : IJob
    {
        private readonly ILogger logger;

        public TestJob(ILogger<TestJob> logger)
        {
            this.logger = logger;
        }

        [Cron("0-59 * * * * ?", enable: true)]
        public void TestTask1()
        {
            logger.LogInformation("执行了");
        }
    }
}
