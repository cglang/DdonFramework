using Ddon.Core.Use.Queue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Core.Tests
{
    [TestClass]
    public class DelayQueueTest
    {
        [TestMethod]
        public async void TestDelayQueue()
        {
            var delayQueue = new DelayQueue<string>();


            var outputs = new Dictionary<string, DateTime>();
            outputs.Add("00", DateTime.Now);


            delayQueue.Add($"50{DateTime.Now}", TimeSpan.FromSeconds(5));
            delayQueue.Add($"20{DateTime.Now}", TimeSpan.FromSeconds(2));
            delayQueue.Add($"20{DateTime.Now}", TimeSpan.FromSeconds(2));
            delayQueue.Add($"120{DateTime.Now}", TimeSpan.FromSeconds(12));
            delayQueue.Add($"21{DateTime.Now}", TimeSpan.FromSeconds(2));

            Assert.AreEqual(4, delayQueue.Count);


            while (delayQueue.Count > 0)
            {
                var item = await delayQueue.TakeAsync(CancellationToken.None);
                if (item != null)
                {
                    Console.WriteLine(item);
                }
            }

            Console.WriteLine(string.Join(Environment.NewLine, outputs.Select(o => $"{o.Key}, {o.Value:HH:mm:ss.ffff}")));

            Assert.AreEqual(2, Calc(outputs.Skip(1).First().Value, outputs.First().Value));
            Assert.AreEqual(2, Calc(outputs.Skip(2).First().Value, outputs.First().Value));
            Assert.AreEqual(5, Calc(outputs.Skip(3).First().Value, outputs.First().Value));
            Assert.AreEqual(12, Calc(outputs.Skip(4).First().Value, outputs.First().Value));
        }


        [TestMethod]
        public async Task TestMultithreading()
        {
            var delayQueue = new DelayQueue<int>();


            var taskCount = 20;
            for (int i = 0; i < taskCount; i++)
            {
                delayQueue.Add(i, TimeSpan.FromSeconds(i + 2));
            }

            Assert.AreEqual(taskCount, delayQueue.Count);


            var outputs = new ConcurrentDictionary<int, int>();
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    while (delayQueue.Count > 0)
                    {
                        var item = await delayQueue.TakeAsync(CancellationToken.None);
                        if (item != default)
                        {
                            outputs.TryAdd(item, Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                }, TaskCreationOptions.LongRunning));
            }

            await Task.WhenAll(tasks);

            Assert.AreEqual(0, delayQueue.Count);
            Assert.AreEqual(taskCount, outputs.Count);

            var preKey = -1;
            foreach (var output in outputs)
            {
                Assert.IsTrue(output.Key > preKey);
                preKey = output.Key;
            }


            Console.WriteLine(string.Join(Environment.NewLine,
                outputs.GroupBy(o => o.Value).Select(g => $"{g.Key}, {g.Count()}")));
        }


        private static int Calc(DateTime dt1, DateTime dt2)
        {
            return (int)(CutOffMillisecond(dt1) - CutOffMillisecond(dt2)).TotalSeconds;
        }


        private static DateTime CutOffMillisecond(DateTime dt)
        {
            return new DateTime(dt.Ticks - dt.Ticks % TimeSpan.TicksPerSecond, dt.Kind);
        }
    }
}
