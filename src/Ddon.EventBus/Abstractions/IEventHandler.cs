using MediatR;

namespace Ddon.EventBus.Contracts
{
    public interface IEventHandler<in IEventData> : INotificationHandler<IEventData>
        where IEventData : INotification
    {
    }
}
