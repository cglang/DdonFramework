using System.Threading;
using System.Threading.Tasks;
using Ddon.Serial.Models;

namespace Ddon.Serial.Abstractions
{
    public interface ISerialHandler
    {
        Task HandleAsync(SerialContext context, CancellationToken cancellationToken = default);
    }
}
