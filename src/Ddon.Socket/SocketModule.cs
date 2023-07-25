using Ddon.Core;
using Ddon.Socket.Core.Storage;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Handler;
using Ddon.Socket.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Socket
{
    public class SocketModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);

            services.AddTransient<SocketInvoke>();

            services.AddSingleton<SocketSessionHandler>();
            services.AddSingleton<SocketServerHandler>();

            services.AddTransient<SocketClientFactory>();

            services.AddSingleton<ISocketSerialize, JsonSocketSerialize>();
            services.AddSingleton<ISocketSessionStorage, SocketSessionStorage>();
        }
    }
}
