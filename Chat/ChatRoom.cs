using System.Collections.Generic;
using System.Threading.Tasks;
using TodoAPI.Middleware;

namespace TodoAPI.Chat
{
    public class ChatRoom : Hub
    {
        public override async Task OnConnected(string clientId)
        {
            var list = new List<string>() { clientId };
            await Clients.AllExecpt(list).SendMessageAsync($"{clientId} joined!");
            await Clients.Client(clientId).SendMessageAsync($"Hello {clientId}");
        }

        public override async Task OnDisconnected(string clientId)
        {
            await Clients.All.SendMessageAsync($"{clientId} left us");
        }

        public override async Task MessageReceived(string clientId, string message)
        {
            await Clients
                    .AllExecpt(
                        new List<string>() { clientId }
                    )
                    .SendMessageAsync($"{clientId}: {message}");
        }
    }
}