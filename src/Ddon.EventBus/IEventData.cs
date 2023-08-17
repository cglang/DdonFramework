using MediatR;

namespace Ddon.EventBus
{
    public interface IDistributedEventData : INotification
    {
    }

    public interface IEventData : INotification
    {
    }
}
