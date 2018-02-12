using System;
using System.Collections;
using System.Collections.Generic;

namespace TodoAPI.Middleware
{
    public class HubRouteBuilder : IEnumerable<HubRoute>
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

        public IEnumerator<HubRoute> GetEnumerator()
        {
            foreach (var item in routes)
            {
                yield return new HubRoute { Route = item.Key, HubType = item.Value };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class HubRoute
    {
        public string Route { get; set; }
        public Type HubType { get; set; }
    }
}