using Ddon.Core.Use.Di;
using Ddon.Socket.Session;

namespace Test.WebApplication.Controllers
{
    /// <summary>
    /// Socket API
    /// </summary>
    public class OpenSocketApi : SocketApiCore, ITransientDependency
    {

        /// <summary>
        /// 开放 API
        /// </summary>
        public OpenSocketApi() { }

        /// <summary>
        /// 获取某天的N条热点
        /// </summary>
        [SocketApi]
        public async Task<IEnumerable<string>> GetAnalysisByDayAsync(AnalysisByDayInput input)
        {
            await Task.Delay(100);

            return Enumerable.Range(0, input.Count).Select(x => x.ToString()); ;
        }
    }
}
