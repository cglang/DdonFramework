using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Schedule
{
    public interface IScheduleHandler<TScheduledEventData>
        where TScheduledEventData : ISchedule
    {
        Task Handle(TScheduledEventData notification, CancellationToken cancellationToken);
    }
}
