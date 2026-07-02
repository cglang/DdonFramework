using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Schedule
{
    public interface ISchedules { }

    public interface ISchedule
    {
        public Task InvokeAsync(CancellationToken cancellationToken);
    }
}
