using Ddon.Core;
using Ddon.Socket.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Socket
{
    public class SocketModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<DdonSocketInvoke>();
        }
    }
}
