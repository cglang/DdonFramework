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
            var client = _socketClientFactory.Create();

            var route = "OpenSocketApi::GenerateSequence";

            return await client.RequestAsync<IEnumerable<string>>(route, new { Count = 1000 });
        }
    }
}
