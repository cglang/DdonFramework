using Ddon.Schedule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test.Job;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, service) =>
    {
        service.LoadModule<ScheduleModule>(context.Configuration);
        service.AddLogging();
        service.AddTransient<JobService>();
    }).RunConsoleAsync();
