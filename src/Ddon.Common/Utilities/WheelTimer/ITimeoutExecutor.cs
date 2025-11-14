using System;
using System.Threading.Tasks;

namespace Ddon.Common.Utilities.WheelTimer
{
    public interface ITimeoutExecutor
    {
        void Execute(Action action);
        void Execute(Func<Task> asyncAction);
    }
}
