using System.Threading.Tasks;

namespace Ddon.Core.Use.Reflection
{
    public interface IDdonServiceInvoke
    {
        Task<dynamic?> IvnvokeAsync<TClass>(string methodName, params object[] parameter) where TClass : class;
    }
}
