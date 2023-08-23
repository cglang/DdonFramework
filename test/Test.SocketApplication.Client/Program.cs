using Ddon.Socket;
using Ddon.Socket.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureServices((context, services) =>
{
    services.LoadModule<SocketModule>(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions().Configure<SocketClientOption>(options =>
{
    options.Host = "127.0.0.1";
    options.Port = 6012;
    options.ConfigureMiddlewares(x =>
    {
        x.UseEndPoints();
    });
});

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
