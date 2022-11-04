using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.Job
{
    public interface IMissionManager
    {
        int Count { get; }

        Task AddAsync(Mission mission);

        Task AddRangeAsync(IEnumerable<Mission> missions);

        Task RemoveAsync(Guid missionId);

        Task RemoveRangeAsync(IEnumerable<Guid> missionIds);

        Task<Mission?> GetAsync(Guid missionId);

        Task<IEnumerable<Mission>> GetRangeAsync(IEnumerable<Guid> missionIds);

        Task<IEnumerable<Mission>> GetAllAsync();

        Task StopAsync(Guid missionId);

        Task StopRangeAsync(IEnumerable<Guid> missionIds);

        Task StartAsync(Guid missionId);

        Task StartRangeAsync(IEnumerable<Guid> missionIds);
    }
}