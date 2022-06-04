using Ddon.Core;
using Ddon.EventBus;
using Ddon.KeyValueStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job
{
    public class JobModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<EventBusModule>(services, configuration);

            services.Configure<DdonJobOptions>(options =>
            {
                options.AutoSave = true;
                options.StorageName = "jobs";
            });

            services.AddSingleton<IDdonKeyValueManager<Job, DdonJobOptions>, DdonKeyValueManager<Job, DdonJobOptions>>();
            services.AddTransient<IDdonJob, DdonJob>();
        }
    }
}
