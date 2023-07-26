using System.Threading.Tasks;

namespace Ddon.Core.Use.Pipeline
{
    public delegate Task MiddlewareDelegate<T>(T context);
}
