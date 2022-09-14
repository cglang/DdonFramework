using Ddon.Socket;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Test.SocketApplication.Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public WeatherForecastController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpGet(Name = "Get")]
        public async Task<IEnumerable<int>> Get()
        {
            var route = "OpenSocketApi::GetAnalysisByDayAsync";
            var data = new { Count = 10 };
            var json = JsonSerializer.Serialize(data);

            var client = SocketClient.CreateClient(_serviceProvider, "127.0.0.1", 2222);

            var aaa = await client.RequestAsync<object>(route, data);

            Console.WriteLine(JsonSerializer.Serialize(aaa));

            return Enumerable.Range(1, 5);
        }
    }
}