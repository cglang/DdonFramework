using System.Threading.Tasks;

namespace Ddon.Core.Use.Pipeline
{
    public interface IGeneralMiddleware<T>
    {
        Task InvokeAsync(T context, MiddlewareDelegate<T> next);
    }
}
