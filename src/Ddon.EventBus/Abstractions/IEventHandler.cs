using System.Threading;
using System.Threading.Tasks;

namespace Ddon.EventBus.Contracts
{
    public interface IEventHandler<in TEvent>
        where TEvent : IEventData
    {
        Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
    }
}
