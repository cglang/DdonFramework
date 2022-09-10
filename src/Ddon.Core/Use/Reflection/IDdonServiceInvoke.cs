using System.Threading.Tasks;

namespace Ddon.Core.Use.Reflection
{
    public interface IDdonServiceInvoke
    {
        Task<dynamic?> InvokeAsync(string className, string methodName, params object[] parameter);

        Task<string?> InvokeGetJsonAsync<TOut>(string className, string methodName, params object[] parameter);

        Task<dynamic?> InvokeAsync(string className, string methodName, string parameter);
    }
}
