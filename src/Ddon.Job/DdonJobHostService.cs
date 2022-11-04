using Ddon.Core.Services.LazyService.Static;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Job
{
    /// <summary>
    /// Job 服务启动
    /// </summary>
    public class DdonJobHostService : IHostedService
    {
        private readonly IMissionManager _missionManager;

        /// <summary>
        /// Job 服务启动
        /// </summary>
        public DdonJobHostService(IServiceProvider serviceProvider, IMissionManager missionManager)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            _missionManager = missionManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var baseType = typeof(IJobElement);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            var implementTypes = types.Where(x => x.IsClass).ToList();

            foreach (var implementType in implementTypes)
            {
                var element = (IJobElement?)Activator.CreateInstance(implementType);
                if (element != null)
                {
                    var mis = new Mission(element.Rule, element.Action);
                    await _missionManager.AddAsync(mis);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
