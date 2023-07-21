using Ddon.Socket;
using Ddon.Socket.Session;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Test.SocketApplication.Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SocketClientFactory socketClientFactory;

        public WeatherForecastController(IServiceProvider serviceProvider, SocketClientFactory socketClientFactory)
        {
            _serviceProvider = serviceProvider;
            this.socketClientFactory = socketClientFactory;
        }

        /// <summary>
        /// ���Ի���
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAnalysisByDayAsync")]
        public async Task<IEnumerable<int>> GetGetAnalysisByDayAsync()
        {
            var route = "OpenSocketApi::GetAnalysisByDayAsync";
            var data = new { Count = 1000 };
            var json = JsonSerializer.Serialize(data);

            var client = socketClientFactory.Create();

            var aaa = await client.RequestAsync<string>(route, data);
            //Console.WriteLine(aaa);
            Console.WriteLine(1);

            return Enumerable.Range(1, 5);
        }

        [HttpGet("SendFile")]
        public async Task<IEnumerable<int>> Get3(string filepath)
        {
            var client = socketClientFactory.Create();

            var route = "OpenSocketApi::ReceiveFile";
            //await client.SendFileAsync(route, filepath);

            return Enumerable.Range(1, 5);
        }
    }
}
