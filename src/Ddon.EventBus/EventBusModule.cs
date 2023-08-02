using System;
using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.EventBus
{
    public class EventBusModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            });
        }
    }
}
