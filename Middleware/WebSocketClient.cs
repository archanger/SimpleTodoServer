using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TodoAPI.Middleware
{
    public class WebSocketClient
    {
        public string Id { get; set; }
        public WebSocket Socket { get; set; }

        public async Task SendMessageAsync(string action, object message) {

            if (message == null) {
                return;
            }

            try
            {
                WSResponse response;
                if (message is string) {
                    response = new WSResponse { Action = action, Data = JObject.FromObject(new { Message =message })};
                } else {
                    response = new WSResponse { Action = action, Data = JObject.FromObject(message) };
                }
                var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                await Socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (System.Exception)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Client was closed", CancellationToken.None);
            }
        }
    }
}