using System.Collections.Generic;

namespace Ddon.Socket.Session.Route
{
    public abstract class DdonSocketRouteMapLoadBase
    {
        internal List<DdonSocketRoute> DdonSocketRoutes => InitRouteMap();

        protected abstract List<DdonSocketRoute> InitRouteMap();
    }
}
