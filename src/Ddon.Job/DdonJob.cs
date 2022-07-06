using Ddon.KeyValueStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.Job
{
    public class DdonJob : IDdonJob
    {
        private readonly IDdonKeyValueManager<Job, DdonJobOptions> _keyValueManager;

        public DdonJob(IDdonKeyValueManager<Job, DdonJobOptions> keyValueManager)
        {
            _keyValueManager = keyValueManager;
        }

        public async Task Add(Job job)
        {
            await _keyValueManager.SetValueAsync(job.Id, job);
            
            job.SetCompleted(async (id) =>
            {
                job.Finish = true;
                job.Stop();
                await _keyValueManager.SetValueAsync(job.Id, job);
            });
            job.Start();
        }

        public async Task<Job?> Get(Guid id)
        {
            return await _keyValueManager.GetValueAsync(id);
        }

        public async Task<IEnumerable<Job>> All()
        {
            return await _keyValueManager.GetAllValueAsync();
        }

        public async Task Remove(Guid id)
        {
            await _keyValueManager.DeleteValueAsync(id.ToString());
        }

        public async Task Update(Job plan)
        {
            await _keyValueManager.SetValueAsync(plan.Id, plan);
        }
    }
}
