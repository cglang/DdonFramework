using Ddon.Core.Use;
using Ddon.Socket;
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

        [HttpGet("Get")]
        public async Task<IEnumerable<int>> Get()
        {
            var route = "OpenSocketApi::GetTopSummariesWeekAsync";
            var data = new { Count = 20 };
            var json = JsonSerializer.Serialize(data);

            using var client = SocketClient.CreateClient(_serviceProvider, "47.105.149.144", 10005);
            var aaa = await client.RequestAsync(route, data);

            return Enumerable.Range(1, 5);
        }

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

        [HttpGet("Get2")]
        public async Task<IEnumerable<int>> Get2()
        {
            var filePath = @"D:\listen1.exe";
            using var source = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var filePath2 = @"D:\listen2.exe";
            using var source2 = new FileStream(filePath2, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            Console.WriteLine(source.Length);

            long size = 0;
            byte[] bytes = new byte[102400];//100 KByte = 0.1 MByte
            int bytesRead;
            while ((bytesRead = source.Read(bytes, 0, bytes.Length)) > 0)
            {
                size += bytesRead;
                source2.Write(bytes, 0, bytesRead);
            }

            Console.WriteLine(size);
            Console.WriteLine("½áÊø");
            return Enumerable.Range(1, 5);
        }

        [HttpGet("Get3")]
        public async Task<IEnumerable<int>> Get3()
        {
            var client = SocketClient.CreateClient(_serviceProvider, "127.0.0.1", 2222);

            var route = "OpenSocketApi::GetAnalysisByDayAsync";
            var filePath = @"D:\cgl.jpg";
            await client.SendFileAsync(route, filePath);

            return Enumerable.Range(1, 5);
        }
    }
}