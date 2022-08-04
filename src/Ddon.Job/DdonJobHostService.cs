using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Services.LazyService.Static;
using Ddon.KeyValueStorage;
using Microsoft.Extensions.Hosting;

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
            var missions = await _missionManager.GetAllAsync();

            foreach (var mission in missions)
            {
                if (mission.State == MissionState.Started)
                {
                    await _missionManager.StartAsync(mission.Id);
                }
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
