using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Models;

namespace Ddon.Socket.Abstractions
{
    public interface ISocketHandler
    {
        Task HandleAsync(SocketContext context, CancellationToken cancellationToken = default);
    }
}
