using Ddon.Core.Services.LazyService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.Job
{
    public class MissionManager : IMissionManager
    {
        private readonly ILazyServiceProvider _lazyServiceProvider;

        public int Count => MissionData.Missions.Count;

        public MissionManager(ILazyServiceProvider lazyServiceProvider)
        {
            _lazyServiceProvider = lazyServiceProvider;
        }

        public async Task AddAsync(Mission mission)
        {
            await Task.CompletedTask;
            MissionData.Missions.Add(mission.Id, mission);
            MissionData.DelayQueue.AddAsync(mission.Id, mission.Rule.Interval).Wait();
            _ = Start();
        }

        public async Task AddRangeAsync(IEnumerable<Mission> missions)
        {
            foreach (var mission in missions)
            {
                await AddAsync(mission);
            }
        }

        public async Task<IEnumerable<Mission>> GetAllAsync()
        {
            await Task.CompletedTask;
            return MissionData.Missions.Values;
        }

        public async Task<Mission?> GetAsync(Guid missionId)
        {
            await Task.CompletedTask;
            if (MissionData.Missions.ContainsKey(missionId))
                return MissionData.Missions[missionId];
            return null;
        }

        public async Task<IEnumerable<Mission>> GetRangeAsync(IEnumerable<Guid> missionIds)
        {
            ICollection<Mission> missions = new List<Mission>();
            foreach (var missionId in missionIds)
            {
                var mission = await GetAsync(missionId);
                if (mission != default)
                {
                    missions.Add(mission);
                }
            }
            return missions;
        }

        public async Task RemoveAsync(Guid missionId)
        {
            var mission = await GetAsync(missionId);
            mission?.Stop();
            MissionData.Missions.Remove(missionId);
        }

        public async Task RemoveRangeAsync(IEnumerable<Guid> missionIds)
        {
            foreach (var missionId in missionIds)
            {
                await RemoveAsync(missionId);
            }
        }

        public async Task StartAsync(Guid missionId)
        {
            var mission = await GetAsync(missionId);
            mission?.Start();
        }

        public async Task StartRangeAsync(IEnumerable<Guid> missionIds)
        {
            foreach (var missionId in missionIds)
            {
                var mission = await GetAsync(missionId);
                mission?.Start();
            }
        }

        public async Task StopAsync(Guid missionId)
        {
            var mission = await GetAsync(missionId);
            mission?.Stop();
        }

        public async Task StopRangeAsync(IEnumerable<Guid> missionIds)
        {
            foreach (var missionId in missionIds)
            {
                var mission = await GetAsync(missionId);
                mission?.Stop();
            }
        }

        private bool state = false;
        private async Task Start()
        {
            if (state) return;

            state = true;
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var item = await MissionData.DelayQueue.TakeAsync();
                    while (item != default)
                    {
                        MissionData.Missions[item].Action.Invoke();
                        await AddAsync(MissionData.Missions[item]);
                    }
                }));
            }

            await Task.WhenAll(tasks);
            state = false;
        }
    }
}
