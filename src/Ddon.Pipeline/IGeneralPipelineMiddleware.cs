using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 常规管道中间件接口
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public interface IGeneralPipelineMiddleware<TContext>
    {
        Task InvokeAsync(TContext context, PipelineDelegate<TContext> next);
    }
}
