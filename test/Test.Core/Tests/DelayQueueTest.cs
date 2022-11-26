//using Ddon.Core.Use.DelayQueue;
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
            var delayQueue = new DelayQueue<Action>();

            // 输出列表
            var outputs = new Dictionary<string, DateTime>();
            outputs.Add("00", DateTime.Now);

            // 添加任务
            await delayQueue.AddAsync(() => { outputs.Add("50", DateTime.Now); }, TimeSpan.FromSeconds(5));
            await delayQueue.AddAsync(() => { outputs.Add("20", DateTime.Now); }, TimeSpan.FromSeconds(2));
            await delayQueue.AddAsync(() => { outputs.Add("20", DateTime.Now); }, TimeSpan.FromSeconds(2));
            await delayQueue.AddAsync(() => { outputs.Add("120", DateTime.Now); }, TimeSpan.FromSeconds(12));
            await delayQueue.AddAsync(() => { outputs.Add("21", DateTime.Now); }, TimeSpan.FromSeconds(2));

            Assert.AreEqual(4, delayQueue.Count);

            // 获取任务
            while (delayQueue.Count > 0)
            {
                var item = delayQueue.Take(CancellationToken.None);
                if (item != null)
                {
                    item.Invoke();
                }
            }

            Console.WriteLine(string.Join(Environment.NewLine, outputs.Select(o => $"{o.Key}, {o.Value:HH:mm:ss.ffff}")));

            Assert.AreEqual(2, Calc(outputs.Skip(1).First().Value, outputs.First().Value));
            Assert.AreEqual(2, Calc(outputs.Skip(2).First().Value, outputs.First().Value));
            Assert.AreEqual(5, Calc(outputs.Skip(3).First().Value, outputs.First().Value));
            Assert.AreEqual(12, Calc(outputs.Skip(4).First().Value, outputs.First().Value));
        }

        /// <summary>
        /// 多线程测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestMultithreading()
        {
            var delayQueue = new DelayQueue<int>();

            // 添加任务
            var taskCount = 20;
            for (int i = 0; i < taskCount; i++)
            {
                await delayQueue.AddAsync(i, TimeSpan.FromSeconds(i + 2));
            }

            Assert.AreEqual(taskCount, delayQueue.Count);

            // 10个线程来消费
            var outputs = new ConcurrentDictionary<int, int>();
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    while (delayQueue.Count > 0)
                    {
                        var item = delayQueue.Take(CancellationToken.None);
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

            // 打印每个线程消费的任务数量
            Console.WriteLine(string.Join(Environment.NewLine,
                outputs.GroupBy(o => o.Value).Select(g => $"{g.Key}, {g.Count()}")));
        }

        private static int Calc(DateTime dt1, DateTime dt2)
        {
            // 毫秒数是存在误差的，这里统计秒数
            return (int)(CutOffMillisecond(dt1) - CutOffMillisecond(dt2)).TotalSeconds;
        }

        /// <summary>
        /// 截掉毫秒部分
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static DateTime CutOffMillisecond(DateTime dt)
        {
            return new DateTime(dt.Ticks - dt.Ticks % TimeSpan.TicksPerSecond, dt.Kind);
        }
    }
}