using System;
using System.Threading.Tasks;
using Ddon.Core.Services.LazyService;
using Ddon.Scheduled;
using Microsoft.Extensions.Logging;
using Test.Job;

namespace Test.Scheduled;

internal class TestJob : IScheduled
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
        _lazyServiceProvider = lazyServiceProvider;
    }

    [Cron("0-59 * * * * ?", enable: true)]
    public async Task TestTask1()
    {
        await Task.Delay(500);
        Console.WriteLine(test.GetHashCode());
        logger.LogInformation("执行了");

        // 在这里面再发送一个事件  事件处理器里就会报错

        var aaa = _lazyServiceProvider.LazyGetRequiredService<JobService>();
        var bbb = _lazyServiceProvider.LazyGetRequiredService<JobService>();
        Console.WriteLine(aaa.GetHashCode());
        Console.WriteLine(bbb.GetHashCode());
    }
}
