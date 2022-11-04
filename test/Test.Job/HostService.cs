using Ddon.Job;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// Job 服务启动
    /// </summary>
    public class HostService : IHostedService
    {
        private readonly IMissionManager _missionManager;

        /// <summary>
        /// Job 服务启动
        /// </summary>
        public HostService(IMissionManager missionManager)
        {
            _missionManager = missionManager;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //if (_missionManager.Count == 0)
            //{
            //    MissionRule rule = new(DateTime.Now, new TimeSpan(0, 0, 5));
            //    Mission mission = new(rule, "ceuiceui");
            //    await _missionManager.AddAsync(mission);
            //}
            //else
            //{
            //    var aaa = await _missionManager.GetAllAsync();
            //    var aa = aaa.FirstOrDefault();
            //    Console.WriteLine(aa!.State);
            //    if (aa!.State == MissionState.Stoped)
            //        await _missionManager.StartAsync(aa.Id);
            //    else
            //        await _missionManager.StopAsync(aa.Id);
            //}


            //var aaa = await _missionManager.GetAllAsync();
            //var aa = aaa.FirstOrDefault();
            //Console.WriteLine(aa!.State);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
