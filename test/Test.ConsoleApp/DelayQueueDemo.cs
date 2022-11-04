using Ddon.Core.Use.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.ConsoleApp
{
    public class DelayQueueDemo
    {
        public static void Run()
        {
            var delayQueue = new DelayQueue<DelayItem<Action>>();

            // 输出列表
            var outputs = new Dictionary<string, DateTime>();
            outputs.Add("00", DateTime.Now);

            // 添加任务
            var item1 = new DelayItem<Action>(TimeSpan.FromSeconds(5), () => { Console.WriteLine(50); });
            var item2 = new DelayItem<Action>(TimeSpan.FromSeconds(2), () => { Console.WriteLine(20); });
            delayQueue.Add(item1);
            delayQueue.Add(item2);
            delayQueue.Add(item2);

            delayQueue.Add(new DelayItem<Action>(TimeSpan.FromSeconds(8), () => { Console.WriteLine(80); }));
            delayQueue.Add(new DelayItem<Action>(TimeSpan.FromSeconds(2), () => { Console.WriteLine(20); }));

            // 获取任务
            while (delayQueue.IsEmpty)
            {
                var task = delayQueue.Take(CancellationToken.None);
                if (task != null)
                {
                    task.Item.Invoke();
                }
            }
        }

        public static async Task SRun()
        {
            var delayQueue = new DelayQueue<DelayItem<Action<int>>>();

            // 添加任务
            var taskCount = 5;
            for (int i = 0; i < taskCount; i++)
            {
                delayQueue.Add(new DelayItem<Action<int>>(TimeSpan.FromSeconds(i + 2), (aa) =>
                {
                    Console.WriteLine(aa);
                }));
            }


            // 10个线程来消费
            var outputs = new ConcurrentDictionary<int, int>();
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var g = Guid.NewGuid();
                    while (delayQueue.Count > 0)
                    {
                        var task = delayQueue.Take(CancellationToken.None);
                        if (task != null)
                        {
                            Console.WriteLine("线程:" + g);
                            task.Item.Invoke(task.GetHashCode());
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            var preKey = -1;
            foreach (var output in outputs)
            {
                preKey = output.Key;
            }

            // 打印每个线程消费的任务数量
            Console.WriteLine(string.Join(Environment.NewLine,
                outputs.GroupBy(o => o.Value).Select(g => $"{g.Key}, {g.Count()}")));
        }
    }
}
