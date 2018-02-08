using System;
using Microsoft.AspNetCore.Builder;

namespace TodoAPI.Middleware
{
    public static class CustomSocketsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomWebSocket(this IApplicationBuilder builder, Action<HubRouteBuilder> configure)
        {
            builder.UseWebSockets(new WebSocketOptions{
                KeepAliveInterval = TimeSpan.FromSeconds(20),
                ReceiveBufferSize = 4 * 1024
            });
            var router = new HubRouteBuilder();
            configure.Invoke(router);
            builder.UseMiddleware<WebSocketMiddleware>(router);
            return builder;
        }
    }
}