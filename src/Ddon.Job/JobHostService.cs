using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Cronos;
using Microsoft.Extensions.Hosting;

namespace Ddon.Job
{
    /// <summary>
    /// Job 服务启动
    /// </summary>
    internal class JobHostService : BackgroundService
    {
        private readonly JobService _jobService;

        /// <summary>
        /// Job 服务启动
        /// </summary>
        public JobHostService(JobService jobService)
        {
            _jobService = jobService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var baseType = typeof(IJob);

            var implementTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.IsClass)
                .ToList();

            foreach (var implementType in implementTypes)
            {
                if (string.IsNullOrWhiteSpace(implementType.FullName)) continue;

                foreach (var method in implementType.GetMethods())
                {
                    var attrs = method.GetCustomAttributes(typeof(CronAttribute), false).As<IEnumerable<CronAttribute>>();
                    foreach (var attr in attrs)
                    {
                        if (attr.Enable == false) continue;

                        var cron = CronExpression.Parse(attr.CronExpression, attr.Format);
                        var jobInvokeData = new JobInvokeData(cron, attr.Zone, attr.Inclusive, implementType.FullName, method.Name);
                        JobData.Jobs.Add(Guid.NewGuid(), jobInvokeData);
                    }
                }
            }

            await _jobService.StartAsync();
        }
    }
}
