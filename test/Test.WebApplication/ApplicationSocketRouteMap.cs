using Ddon.Socket.Session.Route;

namespace Test.WebApplication
{
    /// <summary>
    /// Socket 路由
    /// </summary>
    public class ApplicationSocketRouteMap : DdonSocketRouteMapLoadBase
    {
        /// <summary>
        /// 初始化路由
        /// </summary>
        protected override List<DdonSocketRoute> InitRouteMap()
        {
            var routes = new List<DdonSocketRoute>();

            routes.Add(new("/Api/Open/GetAnalysisByDayAsync", "OpenSocketApi", "GetAnalysisByDayAsync"));

            return routes;
        }
    }
}
