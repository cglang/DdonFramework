using System;
using System.Threading.Tasks;

namespace Ddon.Common.Utilities.WheelTimer
{
    internal class DefaultExecutor : ITimeoutExecutor
    {
        public void Execute(Action action)
        {
            Task.Run(() =>
            {
                try { action(); } catch { }
            });
        }

        public void Execute(Func<Task> asyncAction)
        {
            Task.Run(async () =>
            {
                try { await asyncAction(); } catch { }
            });
        }
    }
}
