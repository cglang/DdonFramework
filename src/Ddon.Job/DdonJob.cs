using Ddon.KeyValueStorage;

namespace Ddon.Job
{
    public class DdonJob : IDdonJob
    {
        private readonly IDdonKeyValueManager<Plan, DdonJobOptions> _keyValueManager;

        public DdonJob(IDdonKeyValueManager<Plan, DdonJobOptions> keyValueManager)
        {
            _keyValueManager = keyValueManager;
            var allValueTask = keyValueManager.GetAllValueAsync();
            allValueTask.Wait();
            Parallel.ForEach(allValueTask.Result, value =>
            {
                value.Start();
            });
        }

        public async Task Add(Plan plan)
        {
            await _keyValueManager.SetValueAsync(plan.Id, plan);
            plan.Start();
        }

        public async Task<IEnumerable<Plan>> All()
        {
            return await _keyValueManager.GetAllValueAsync();
        }

        public async Task Remove(Guid id)
        {
            await _keyValueManager.DeleteValueAsync(id.ToString());
        }

        public async Task Update(Plan plan)
        {
            await _keyValueManager.SetValueAsync(plan.Id, plan);
        }
    }
}
