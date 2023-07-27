using System.Diagnostics;
using Ddon.Socket;
using Microsoft.AspNetCore.Mvc;

namespace Test.SocketApplication.Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly SocketClientFactory _socketClientFactory;

        public WeatherForecastController(SocketClientFactory socketClientFactory)
        {
            _socketClientFactory = socketClientFactory;
        }

        [HttpGet("GetGenerateSequenceAsync")]
        public async Task<IEnumerable<string>?> GetGenerateSequenceAsync()
        {
            using var client = _socketClientFactory.Create();

            var route = "OpenSocketApi::GenerateSequence";

            List<string> result = new List<string>();

            var watch = Stopwatch.StartNew();

            var data1 = await client.RequestAsync<IEnumerable<string>>(route, new { Count = 1000 });
            result.AddRange(data1!);

            var data2 = await client.RequestAsync<IEnumerable<string>>(route, new { Count = 1000 });
            result.AddRange(data2!);

            watch.Stop();
            Console.WriteLine(watch.Elapsed.Milliseconds);

            return result;
        }
    }
}
