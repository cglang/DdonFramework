using System.Net;
using Ddon.Socket;
using Test.WebApplication.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureServices((context, services) =>
{
    services.LoadModule<SocketModule>(context.Configuration);
    services.AddSocketServerService(context.Configuration, opt =>
    {
        opt.IPEndPoint = new(IPAddress.Loopback, 6012);
        opt.ConfigureMiddlewares(x =>
        {
            x.UseEndPoints();
        });
    });
});


builder.Services.AddControllers();
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
