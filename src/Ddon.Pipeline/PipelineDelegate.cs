using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    public delegate Task PipelineDelegate<T>(T context);
}
