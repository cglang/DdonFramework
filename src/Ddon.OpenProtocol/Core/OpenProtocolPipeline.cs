using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Models;
using Ddon.Pipeline;

namespace Ddon.OpenProtocol.Core
{
    public class OpenProtocolPipeline : IOpenProtocolPipeline
    {
        private readonly IGeneralCustomPipeline<OpenProtocolContext> _pipeline;

        public OpenProtocolPipeline(IGeneralCustomPipeline<OpenProtocolContext> pipeline)
        {
            _pipeline = pipeline;
        }

        public Task ExecuteAsync(OpenProtocolContext context)
        {
            return _pipeline.ExecuteAsync(context);
        }
    }
}
