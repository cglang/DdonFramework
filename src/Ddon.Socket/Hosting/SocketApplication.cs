using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Socket.Hosting
{
    // TODO: IHost 还需要看
    public sealed class SocketApplication
    {
        public static SocketApplicationBuilder CreateBuilder(string[] args)
        {
            return new();
        }

        public void Run(string host, int port)
        {

        }

        public void Run(int? port) => Run("0.0.0.0", port ?? 6000);
    }

    public class SocketApplicationBuilder
    {
        public static SocketApplication Build()
        {
            SocketApplication application = new();
            return application;
        }
    }
}
