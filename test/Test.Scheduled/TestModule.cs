using Ddon.Core;
using Ddon.Schedule;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Job;

namespace Test.Scheduled
{
    internal class TestModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<ScheduleModule, ScheduleServiceConfiguration>(services, configuration, config =>
            {
                config.Add<TestJob>("0-59 * * * * ?");
            });
            services.AddLogging();
            services.AddTransient<JobService>();
        }
    }
}
