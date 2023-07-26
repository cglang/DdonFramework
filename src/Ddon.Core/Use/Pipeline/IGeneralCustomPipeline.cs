using System.Threading.Tasks;

namespace Ddon.Core.Use.Pipeline
{
    public interface IGeneralCustomPipeline<T>
    {
        Task ExecuteAsync(T context);
    }
}
