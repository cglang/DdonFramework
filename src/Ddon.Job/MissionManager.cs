using Ddon.KeyValueStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ddon.Job
{
    public class MissionManager : IMissionManager
    {
        private readonly IDdonKeyValueManager<Mission, JobOptions> _keyValueManager;

        public MissionManager(IDdonKeyValueManager<Mission, JobOptions> keyValueManager)
        {
            _keyValueManager = keyValueManager;
        }

        public int Count
        {
            get
            {
                var keysTask = _keyValueManager.GetAllKeyAsync();
                keysTask.Wait();
                return keysTask.Result.Count();
            }
        }

        public async Task AddAsync(Mission mission)
        {
            await _keyValueManager.SetValueAsync(mission.Id, mission);
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
            return await _keyValueManager.GetAllValueAsync();
        }

        public async Task<Mission?> GetAsync(Guid missionId)
        {
            return await _keyValueManager.GetValueAsync(missionId.ToString());
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
            await _keyValueManager.DeleteValueAsync(missionId.ToString());
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

            await _keyValueManager.SaveAsync();
        }

        public async Task StartRangeAsync(IEnumerable<Guid> missionIds)
        {
            foreach (var missionId in missionIds)
            {
                var mission = await GetAsync(missionId);
                mission?.Start();
            }

            await _keyValueManager.SaveAsync();
        }

        public async Task StopAsync(Guid missionId)
        {
            var mission = await GetAsync(missionId);
            mission?.Stop();

            await _keyValueManager.SaveAsync();
        }

        public async Task StopRangeAsync(IEnumerable<Guid> missionIds)
        {
            foreach (var missionId in missionIds)
            {
                var mission = await GetAsync(missionId);
                mission?.Stop();
            }

            await _keyValueManager.SaveAsync();
        }

        public async Task UpdateAsync(Mission mission)
        {
            await _keyValueManager.SetValueAsync(mission.Id, mission);
        }

        public async Task UpdateRangeAsync(IEnumerable<Mission> missions)
        {
            foreach (var mission in missions)
            {
                await UpdateAsync(mission);
            }
        }
    }
}
