using Ddon.Core.Services.LazyService.Static;
using Ddon.KeyValueStorage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ddon.Job
{
    public class DdonJobService
    {
        public static void Start(IServiceProvider serviceProvider, IDdonKeyValueManager<Job, DdonJobOptions> keyValueManager)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);

            var allValueTask = keyValueManager.GetAllValueAsync();
            allValueTask.Wait();
            Console.WriteLine($"job个数{allValueTask.Result.Count()}");
            Parallel.ForEach(allValueTask.Result, value =>
            {
                if (value.Finish == false)
                {
                    value.Start();
                }
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
