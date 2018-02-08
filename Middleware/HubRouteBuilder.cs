using System;
using System.Collections.Generic;

namespace TodoAPI.Middleware
{
    public class HubRouteBuilder
    {
        private readonly Dictionary<string, Type> routes;
        public HubRouteBuilder()
        {
            this.routes = new Dictionary<string, Type>();
        }

        public void MapHub<THub>(string hubName) where THub : Hub
        {
            var t = typeof(THub);
            routes[hubName] = t;
        }

        public bool CanAcceptRoute(string route) 
        {
            return routes.ContainsKey(route);
        }

        public Type TypeForRoute(string route)
        {
            if (!CanAcceptRoute(route)) {
                return null;
            }
            return routes[route];
        }
    }
}