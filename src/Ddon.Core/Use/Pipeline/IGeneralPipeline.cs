using System.Threading.Tasks;

namespace Ddon.Core.Use.Pipeline
{
    public interface IGeneralPipeline<T>
    {
        Task InvokeAsync(T context, PipelineDelegate<T> next);
    }
}
