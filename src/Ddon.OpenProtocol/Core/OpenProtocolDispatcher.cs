using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Models;

namespace Ddon.OpenProtocol.Core
{
    public class OpenProtocolDispatcher
    {
        private readonly List<IOpenProtocolHandler> _handlers;

        public OpenProtocolDispatcher(IEnumerable<IOpenProtocolHandler> handlers)
        {
            _handlers = new List<IOpenProtocolHandler>(handlers);
        }

        public async Task DispatchAsync(
            OpenProtocolContext context,
            CancellationToken cancellationToken = default)
        {
            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(context, cancellationToken);
            }
        }
    }
}
