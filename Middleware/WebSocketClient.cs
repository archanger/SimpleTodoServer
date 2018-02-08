using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TodoAPI.Middleware
{
    public class WebSocketClient
    {
        public string Id { get; set; }
        public WebSocket Socket { get; set; }

        public async Task SendMessageAsync(string message) {
            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await Socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}