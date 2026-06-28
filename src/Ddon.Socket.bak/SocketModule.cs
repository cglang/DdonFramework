using Ddon.Core;
using Ddon.Socket.Core.Storage;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Handler;
using Ddon.Socket.Session.Middleware;
using Ddon.Socket.Session.Pipeline;
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

            services.AddTransient<SocketSessionHandler>();
            services.AddTransient<SocketServerHandler>();

            services.AddTransient<SocketClientFactory>();

            services.AddSingleton<ISocketSerialize, JsonSocketSerialize>();
            services.AddSingleton<ISocketSessionStorage, SocketSessionStorage>();

            services.AddSingleton<ISocketMiddlewarePipelineRegistrar, SocketMiddlewarePipelineRegistrar>();
            services.AddSingleton<ISocketByteCustomPipeline, SocketByteCustomPipeline>();

            services.AddTransient<RouteMiddleware>();
            services.AddTransient<EndPointMiddleware>();
        }
    }
}
