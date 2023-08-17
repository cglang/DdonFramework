using MediatR;

namespace Ddon.EventBus
{
    public interface IEventHandler<in IEventData> : INotificationHandler<IEventData>
        where IEventData : INotification
    {
    }
}
