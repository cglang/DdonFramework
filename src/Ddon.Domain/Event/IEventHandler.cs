using MediatR;

namespace Ddon.Domain.Event
{
    public interface IEventHandler<in IEventData>: INotificationHandler<IEventData>
        where IEventData : INotification
    {
    }
}
