using MediatR;

namespace Ddon.Schedule
{
    public interface IScheduleHandler<TScheduledEventData> : INotificationHandler<TScheduledEventData>
        where TScheduledEventData : ISchedule
    {
    }
}
