using Ddon.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, service) =>
    {
        service.LoadModule<JobModule>(context.Configuration);
        service.AddLogging();
    }).RunConsoleAsync();
