using System.Threading.Tasks;

namespace Ddon.Core.Use.Pipeline
{
    public delegate Task PipelineDelegate<T>(T context);
}
