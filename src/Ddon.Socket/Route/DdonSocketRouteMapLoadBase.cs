namespace Ddon.Socket.Route
{
    public abstract class DdonSocketRouteMapLoadBase
    {
        internal List<DdonSocketRoute> DdonSocketRoutes => InitRouteMap();

        protected abstract List<DdonSocketRoute> InitRouteMap();
    }
}
