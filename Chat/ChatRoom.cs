using System.Collections.Generic;
using System.Threading.Tasks;
using TodoAPI.Middleware;

namespace TodoAPI.Chat
{
    public class ChatRoom : Hub
    {
        public ChatRoom(Clinets clients) : base(clients)
        {
        }

        public override async Task OnConnected(WebSocketClient client)
        {
            var list = new List<string>() { client.Id };
            await Clients.AllExecpt(list).SendMessageAsync($"{client.Id} joined!");
            await Clients.Client(client.Id).SendMessageAsync($"Hello {client.Id}");
        }

        public override async Task OnDisconnected(WebSocketClient client)
        {
            await client.SendMessageAsync($"Left! {client.Id}");
        }

        public override async Task MessageReceived(WebSocketClient client, string message)
        {
            await client.SendMessageAsync("Got it");
        }
    }
}