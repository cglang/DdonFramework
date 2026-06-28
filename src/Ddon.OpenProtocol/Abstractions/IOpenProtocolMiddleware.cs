using Ddon.Pipeline;
using Ddon.OpenProtocol.Models;

namespace Ddon.OpenProtocol.Abstractions
{
    public interface IOpenProtocolMiddleware : IGeneralPipelineMiddleware<OpenProtocolContext>
    {
    }
}
