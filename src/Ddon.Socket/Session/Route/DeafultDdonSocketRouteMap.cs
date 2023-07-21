using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ddon.Socket.Session.Route
{
    public class DeafultDdonSocketRouteMap : DdonSocketRouteMapLoadBase
    {
        protected override List<DdonSocketRoute> InitRouteMap()
        {
            var routes = new List<DdonSocketRoute>();

            var baseType = typeof(SocketApiBase);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type)).ToList();

            foreach (var type in types)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var socketApi = method.GetCustomAttribute<SocketApiAttribute>();
                    if (socketApi == null) continue;

                    var routeText = $"{type.Name}::{socketApi.Template ?? method.Name}";
                    var route = new DdonSocketRoute(routeText, type.Name, method.Name);
                    routes.Add(route);
                }
            }

            return routes;
        }
    }
}
