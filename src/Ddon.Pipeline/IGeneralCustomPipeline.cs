using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 常规自定义管道接口
    /// </summary>
    /// <typeparam name="T">管道上下文数据</typeparam>
    public interface IGeneralCustomPipeline<T>
    {
        Task ExecuteAsync(T context);
    }
}
