using Ddon.Core.Use.DelayQueue;
using System;
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
            delayQueue.TryAdd(item1);
            delayQueue.TryAdd(item2);
            delayQueue.TryAdd(item2);

            delayQueue.TryAdd(new DelayItem<Action>(TimeSpan.FromSeconds(8), () => { Console.WriteLine(80); }));
            delayQueue.TryAdd(new DelayItem<Action>(TimeSpan.FromSeconds(2), () => { Console.WriteLine(20); }));

            // 获取任务
            while (delayQueue.Count > 0)
            {
                if (delayQueue.TryTake(out var task))
                {
                    task.Item.Invoke();
                }
            }
        }
    }
}
