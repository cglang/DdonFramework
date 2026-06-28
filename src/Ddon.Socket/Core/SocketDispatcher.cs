using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Models;

namespace Ddon.Socket.Core
{
    public class SocketDispatcher
    {
        private readonly List<ISocketHandler> _handlers;

        public SocketDispatcher(IEnumerable<ISocketHandler> handlers)
        {
            _handlers = new List<ISocketHandler>(handlers);
        }

        public async Task DispatchAsync(SocketContext context, CancellationToken cancellationToken = default)
        {
            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(context, cancellationToken);
            }
        }
    }
}
