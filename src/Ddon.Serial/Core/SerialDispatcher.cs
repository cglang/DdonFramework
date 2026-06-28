using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Models;

namespace Ddon.Serial.Core
{
    public class SerialDispatcher
    {
        private readonly List<ISerialHandler> _handlers;

        public SerialDispatcher(IEnumerable<ISerialHandler> handlers)
        {
            _handlers = new List<ISerialHandler>(handlers);
        }

        public async Task DispatchAsync(SerialContext context, CancellationToken cancellationToken = default)
        {
            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(context, cancellationToken);
            }
        }
    }
}
