using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TodoAPI.Chat;
using TodoAPI.Config;
using TodoAPI.Middleware.Implemetations;
using Newtonsoft.Json;
using TodoAPI.Helpers;
using Newtonsoft.Json.Linq;
using System.Linq;
using TodoAPI.Core;
using Microsoft.Extensions.DependencyInjection;

namespace TodoAPI.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Dictionary<string, WebSocketClient> clients;
        private readonly HubRouteBuilder router;
        private readonly IServiceProvider serviceProvider;
        public WebSocketMiddleware(RequestDelegate next, HubRouteBuilder router, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
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


            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var ct = context.RequestAborted;
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            var client = new WebSocketClient { Id = Guid.NewGuid().ToString(), Socket = socket };
            clients[client.Id] = client;

            // context.RequestServices

            var hubType = router.TypeForRoute(context.Request.Path);
            var hub = (Hub)serviceProvider.GetService(hubType);

            hub.Clients = new HubClinets(clients);

            // connected
            await hub.OnConnected(client.Id);

            try
            {

                while (socket.State == WebSocketState.Open)
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

                    await HandleAction(hub, client, response);
                }

            }
            catch (System.Exception)
            {
            }

            // disconnected 

            clients.Remove(client.Id);
            await hub.OnDisconnected(client.Id);
            await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
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

        private async Task HandleAction(Hub hub, WebSocketClient client, string response)
        {
            var requestObject = JsonConvert.DeserializeObject<WSRequest>(response);
            var actionToCall = "On" + requestObject.Action.ToFirstLetterUppercased();
            var hubMethod = hub.GetType().GetMethod(actionToCall);

            if (hubMethod == null)
            {
                await Fallback(hub, client.Id, response);
                return;
            }

            var hubMethodParameters = hubMethod.GetParameters();
            if (hubMethodParameters.Count() != 2)
            {
                await Fallback(hub, client.Id, response);
                return;
            }

            var parameterType = hubMethodParameters.Last().ParameterType;
            var castMethod = requestObject.Data.GetType().GetMethods()
                .FirstOrDefault(method => method.Name == "ToObject" && method.GetParameters().Count() == 0)
                .MakeGenericMethod(new Type[] { parameterType });
            var reauestMappedObject = castMethod.Invoke(requestObject.Data, null);
            var parametersToHubMethod = new object[] {
                        client.Id,
                        reauestMappedObject
                    };
            hubMethod.Invoke(hub, parametersToHubMethod);
        }

        private async Task Fallback(Hub hub, string clientId, string response)
        {
            await hub.MessageReceived(clientId, response);
        }
    }

    public class WSRequest
    {
        public string Action { get; set; }
        public JObject Data { get; set; }
    }

    public class WSResponse
    {
        public string Action { get; set; }
        public JObject Data { get; set; }
    }
}