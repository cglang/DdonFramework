using Ddon.Scheduled;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test.Job;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, service) =>
    {
        //service.LoadModule<ScheduledModule>(context.Configuration);
        service.AddLogging();
        service.AddTransient<JobService>();
    }).RunConsoleAsync();
