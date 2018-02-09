using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TodoAPI.Chat;
using TodoAPI.Middleware.Implemetations;

namespace TodoAPI.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Dictionary<string, WebSocketClient> clients;
        private readonly HubRouteBuilder router;
        public WebSocketMiddleware(RequestDelegate next, HubRouteBuilder router)
        {
            this.router = router;
            this.next = next;
            this.clients = new Dictionary<string, WebSocketClient>();
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest || !router.CanAcceptRoute(context.Request.Path))
            {
                await next(context);
                return;
            }


            if (context.WebSockets.IsWebSocketRequest)
            {
                var ct = context.RequestAborted;
                WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
                var client = new WebSocketClient { Id = Guid.NewGuid().ToString(), Socket = socket };
                clients[client.Id] = client;

                var hub = (Hub)Activator.CreateInstance(
                    router.TypeForRoute(context.Request.Path)
                );
                hub.Clients = new HubClinets(clients);

                // connected
                await hub.OnConnected(client.Id);

                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    var response = await ReceiveStringAsync(client, ct);
                    if (string.IsNullOrEmpty(response))
                    {
                        if (client.Socket.State != WebSocketState.Open)
                        {
                            break;
                        }
                        continue;
                    }

                    await hub.MessageReceived(client.Id, response);

                }

                // disconnected 

                await hub.OnDisconnected(client.Id);

                clients.Remove(client.Id);
                await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
            else
            {
                context.Response.StatusCode = 400;
            }

        }

        private async Task<string> ReceiveStringAsync(WebSocketClient client, CancellationToken token = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[10224 * 8]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    token.ThrowIfCancellationRequested();
                    result = await client.Socket.ReceiveAsync(buffer, token);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}