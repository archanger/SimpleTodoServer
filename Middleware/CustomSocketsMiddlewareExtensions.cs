using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TodoAPI.Core;

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

            var scope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            builder.UseMiddleware<WebSocketMiddleware>(router, scope.ServiceProvider);
            return builder;
        }
    }
}