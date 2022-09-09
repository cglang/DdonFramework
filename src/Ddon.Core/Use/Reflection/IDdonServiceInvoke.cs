using System.Threading.Tasks;

namespace Ddon.Core.Use.Reflection
{
    public interface IDdonServiceInvoke
    {
        Task<dynamic?> IvnvokeAsync(string className, string methodName, params object[] parameter);

        Task<string?> IvnvokeGetJsonAsync<TOut>(string className, string methodName, params object[] parameter);
    }
}
