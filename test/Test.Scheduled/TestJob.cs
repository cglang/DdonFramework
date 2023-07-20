using System;
using System.Threading.Tasks;
using Ddon.Scheduled;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test.Job;

namespace Test.Scheduled;

internal class TestJob : IScheduled
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly JobService _jobService;

    public TestJob(IServiceProvider serviceProvider, ILogger<TestJob> logger, JobService jobService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _jobService = jobService;
    }

    [Cron("0-59 * * * * ?", enable: true)]
    public async Task TestTask1()
    {
        await Task.Delay(500);
        Console.WriteLine(_jobService.GetHashCode());
        _logger.LogInformation("执行了");

        // 在这里面再发送一个事件  事件处理器里就会报错

        var aaa = _serviceProvider.GetRequiredService<JobService>();
        var bbb = _serviceProvider.GetRequiredService<JobService>();
        Console.WriteLine(aaa.GetHashCode());
        Console.WriteLine(bbb.GetHashCode());
    }
}
