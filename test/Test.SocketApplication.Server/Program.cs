using Ddon.FileStorage;
using Ddon.Socket;
using Test.SocketApplication.Server;
using Test.WebApplication.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.ConfigureServices((context, services) =>
{
    services.LoadModule<SocketModule>(context.Configuration);
    services.LoadModule<FileStorageModule>(context.Configuration);
    services.AddHostedService<SocketService>();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<OpenSocketApi>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();



var builder2 = Host.CreateDefaultBuilder(args);

builder2.ConfigureAppConfiguration(app => { });

builder2.ConfigureServices((context, services) =>
{
    services.LoadModule<SocketModule>(context.Configuration);
});

var app2 = builder2.Build();

app2.RunAsync();
