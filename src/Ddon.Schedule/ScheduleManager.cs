using System;
using System.Threading.Tasks;

namespace Ddon.Schedule
{
    public interface IScheduleManager
    {
        Task AddAsync(Guid id, ScheduleInvokeData data);

        Task RemoveAsync(Guid id);

        Task<ScheduleInvokeData> GetAsync(Guid id);
    }

    public class ScheduleManager : IScheduleManager
    {
        public Task AddAsync(Guid id, ScheduleInvokeData data)
        {
            return Task.Run(() =>
            {
                ScheduleData.Schedules.Add(id, data);
                ScheduleData.DelayQueue.Add(id, data.NextSpan);
            });
        }

        public Task<ScheduleInvokeData> GetAsync(Guid id)
        {
            return Task.Run(() => ScheduleData.Schedules[id]);
        }

        public Task RemoveAsync(Guid id)
        {
            return Task.Run(() =>
            {
                ScheduleData.Schedules.Remove(id);
                ScheduleData.DelayQueue.Remove(id);
            });
        }
    }
}
