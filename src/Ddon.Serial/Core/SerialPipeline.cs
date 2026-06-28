using System.Threading.Tasks;
using Ddon.Pipeline;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Models;

namespace Ddon.Serial.Core
{
    public class SerialPipeline : ISerialPipeline
    {
        private readonly IGeneralCustomPipeline<SerialContext> _pipeline;

        public SerialPipeline(IGeneralCustomPipeline<SerialContext> pipeline)
        {
            _pipeline = pipeline;
        }

        public Task ExecuteAsync(SerialContext context)
        {
            return _pipeline.ExecuteAsync(context);
        }
    }
}
