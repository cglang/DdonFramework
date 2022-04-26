using Ddon.Core;
using Ddon.KeyValueStorage;

namespace Ddon.Job
{
    public class DdonJobService
    {
        public static void Start(IServiceProvider serviceProvider, IDdonKeyValueManager<Job, DdonJobOptions> keyValueManager)
        {
            DdonServiceProvider.InitServiceProvider(serviceProvider);

            var allValueTask = keyValueManager.GetAllValueAsync();
            allValueTask.Wait();
            Parallel.ForEach(allValueTask.Result, value =>
            {
                if (value.Finish == false)
                    value.Start();
                value.SetCompleted(async (id) =>
                {
                    value.Finish = true;
                    value.Stop();
                    await keyValueManager.SetValueAsync(value.Id, value);
                });
            });
        }
    }
}
