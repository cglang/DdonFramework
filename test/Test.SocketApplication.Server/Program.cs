using Ddon.Socket;
using Ddon.Socket.Hosting;
using Test.WebApplication.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.ConfigureServices((context, services) =>
{
    services.LoadModule<SocketModule>(context.Configuration);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<OpenSocketApi>();


var app = builder.Build();

var socketBuilder = SocketApplication.CreateBuilder(args, app.Services);
socketBuilder.Configure(config =>
{
    config.SetListenerInfo(2222);
    config.AddExceptionHandler(async (a, b) =>
    {
        Console.WriteLine($"出现了异常:{b.Message}");
        await Task.CompletedTask;
    });
    config.AddSocketAccessHandler(async (a, b) =>
    {
        Console.WriteLine($"客户端接入{a.Conn.SocketId}");
        await Task.CompletedTask;
    });
});
socketBuilder.Build().Run();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
