using System;
using System.Linq;
using Ddon.Core;
using Ddon.Core.Use.Di;
using Ddon.EventBus.MemoryQueue;
using Ddon.Job.old;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job
{
    public class JobModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<EventBusMemoryModule>(services, configuration);
            services.AddHostedService<JobHostService>();
            services.AddSingleton<JobService>();

            var baseType = typeof(IJob);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();
            implementTypes.ForEach(implementType =>
            {
                services.AddTransient(implementType);
            });
        }
    }
}
