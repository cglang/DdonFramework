using Ddon.Core;
using Ddon.EventBus.MemoryQueue;
using Ddon.KeyValueStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job
{
    public class JobModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<EventBusMemoryModule>(services, configuration);

            services.Configure<JobOptions>(options =>
            {
                options.AutoSave = true;
                options.StorageName = "jobs";
            });

            services.AddSingleton<IMissionManager, MissionManager>();
            services.AddSingleton<IDdonKeyValueManager<Mission, JobOptions>, DdonKeyValueManager<Mission, JobOptions>>();
        }
    }
}
