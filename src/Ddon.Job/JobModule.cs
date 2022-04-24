using Ddon.Core;
using Ddon.KeyValueStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job
{
    public class JobModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DdonJobOptions>(options =>
            {
                options.AutoSave = true;
                options.Directory = "Job";
                options.StorageName = "jobs";
            });

            services.AddSingleton<IDdonKeyValueManager<Plan, DdonJobOptions>, DdonKeyValueManager<Plan, DdonJobOptions>>();
            services.AddTransient<IDdonJob, DdonJob>();
        }
    }
}
