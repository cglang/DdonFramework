using Ddon.Socket;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Test.WebApplication.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var route = "/Api/Open/GetAnalysisByDayAsync";
            var data = new { Count = 10 };
            var json = JsonSerializer.Serialize(data);

            IServiceCollection services = new ServiceCollection();
            var sp = services.BuildServiceProvider();


            var client = SocketClient<DdonSocketRouteMapLoad>.CreateClient(sp, "127.0.0.1", 2222);

            var aaa = await client.RequestAsync<object>(route, data);

            Console.WriteLine(JsonSerializer.Serialize(aaa));
        }

        class DdonSocketRouteMapLoad : DdonSocketRouteMapLoadBase
        {
            protected override List<DdonSocketRoute> InitRouteMap()
            {
                return new();
            }
        }
    }
}