using Ddon.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, service) =>
    {
        service.AddHostedService<HostService>();
        service.AddHostedService<DdonJobHostService>();
        service.LoadModule<JobModule>(context.Configuration);
    }).RunConsoleAsync();
