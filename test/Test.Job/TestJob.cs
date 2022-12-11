using System;
using System.Threading.Tasks;
using Ddon.Core.Services.LazyService;
using Ddon.Job;
using Microsoft.Extensions.Logging;

namespace Test.Job
{
    internal class TestJob : IJob
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;
        private readonly JobService test;
        private readonly ILazyServiceProvider _lazyServiceProvider;

        public TestJob(IServiceProvider serviceProvider, ILogger<TestJob> logger, JobService test, ILazyServiceProvider lazyServiceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.test = test;
            this._lazyServiceProvider = lazyServiceProvider;
        }

        [Cron("0-59 * * * * ?", enable: true)]
        public async Task TestTask1()
        {
            await Task.Delay(500);
            System.Console.WriteLine(test.GetHashCode());
            logger.LogInformation("执行了");

            // 在这里面再发送一个事件  事件处理器里就会报错

            var aaa = _lazyServiceProvider.LazyGetRequiredService<JobService>();
            var bbb = _lazyServiceProvider.LazyGetRequiredService<JobService>();
            Console.WriteLine(aaa.GetHashCode());
            Console.WriteLine(bbb.GetHashCode());
        }
    }
}
