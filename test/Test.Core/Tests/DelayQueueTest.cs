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
        public async Task TestDelayQueue()
        {
            var delayQueue = new DelayQueue<string>();

            string[] outs = new string[] { "2", "2", "2", "5", "6" };

            delayQueue.Add($"5", TimeSpan.FromSeconds(5));
            delayQueue.Add($"2", TimeSpan.FromSeconds(2));
            delayQueue.Add($"2", TimeSpan.FromSeconds(2));
            delayQueue.Add($"6", TimeSpan.FromSeconds(6));
            delayQueue.Add($"2", TimeSpan.FromSeconds(2));

            Assert.AreEqual(5, delayQueue.Count);


            for (int i = 0; i < delayQueue.Count; i++)
            {
                var item = await delayQueue.TakeAsync(CancellationToken.None);
                if (item != null)
                {
                    Assert.AreEqual(outs[i], item);
                }
            }
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
