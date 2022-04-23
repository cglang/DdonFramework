using Ddon.Core;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ddon.EventBus
{
    public class EventBusModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
