using Ddon.Socket;
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

SocketServer.CreateServer(app.Services, 2222).Start();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();