using Ddon.Pipeline;
using Ddon.Serial.Models;

namespace Ddon.Serial.Abstractions
{
    public interface ISerialMiddleware : IGeneralPipelineMiddleware<SerialContext>
    {
    }
}
