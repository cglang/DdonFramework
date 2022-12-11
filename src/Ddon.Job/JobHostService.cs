using Ddon.Core.Services.LazyService.Static;
using Ddon.Core.Use.Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Job.old
{
    /// <summary>
    /// Job 服务启动
    /// </summary>
    internal class JobHostService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly JobService _jobService;

        /// <summary>
        /// Job 服务启动
        /// </summary>
        public JobHostService(IServiceProvider serviceProvider, JobService jobService)
        {
            _serviceProvider = serviceProvider;
            _jobService = jobService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

            //using var scope = _serviceProvider.CreateScope();
            _jobService.Start();

            return Task.CompletedTask;
        }
    }
}
