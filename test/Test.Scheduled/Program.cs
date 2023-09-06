using Microsoft.Extensions.Hosting;
using Test.Scheduled;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, service) =>
{
    service.LoadModule<TestModule>(context.Configuration);
});

var app = builder.Build();

await app.RunAsync();
