using Ddon.Core.Use;
using Ddon.Socket;
using Ddon.Socket.Session;
using Microsoft.AspNetCore.Mvc;
using System;
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

        /// <summary>
        /// µ÷ÊÔ»ù´¡
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAnalysisByDayAsync")]
        public async Task<IEnumerable<int>> GetGetAnalysisByDayAsync()
        {
            var route = "OpenSocketApi::GetAnalysisByDayAsync";
            var data = new { Count = 1000 };
            var json = JsonSerializer.Serialize(data);

            using var client = SocketClient.CreateClient(_serviceProvider, "127.0.0.1", 2222);

            var aaa = await client.RequestAsync(route, data);
            //Console.WriteLine(aaa);
            Console.WriteLine(1);

            return Enumerable.Range(1, 5);
        }

        [HttpGet("SendFile")]
        public async Task<IEnumerable<int>> Get3(string filepath)
        {
            var client = SocketClient.CreateClient(_serviceProvider, "127.0.0.1", 2222);

            var route = "OpenSocketApi::ReceiveFile";
            await client.SendFileAsync(route, filepath);

            return Enumerable.Range(1, 5);
        }
    }
}