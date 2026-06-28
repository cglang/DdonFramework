using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Models;

namespace Ddon.OpenProtocol.Abstractions
{
    public interface IOpenProtocolHandler
    {
        Task HandleAsync(OpenProtocolContext context, CancellationToken cancellationToken = default);
    }
}
