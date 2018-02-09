using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TodoAPI.Middleware.Implemetations
{
    public class HubClinets : IHubClients
    {
        private Dictionary<string, WebSocketClient> _clients;

        public IClientProxy All
        {
            get { return new ClinetsProxy(_clients); }
        }

        public HubClinets(Dictionary<string, WebSocketClient> clients)
        {
            this._clients = clients;
        }

        public IClientProxy AllExecpt(IEnumerable<string> clientsIds)
        {
            var filtered = _clients.Where(c => !clientsIds.Contains(c.Key)).ToDictionary(k => k.Key, v => v.Value);
            return new ClinetsProxy(filtered);
        }

        public IClientProxy Client(string clientId)
        {
            WebSocketClient client;
            if (!_clients.TryGetValue(clientId, out client)) {
                return null;
            }
            return new ClientProxy(client);
        }

    }

    public class ClinetsProxy : IClientProxy
    {
        private Dictionary<string, WebSocketClient> _clients;
        public ClinetsProxy(Dictionary<string, WebSocketClient> clients)
        {
            this._clients = clients;
        }
        public async Task SendMessageAsync(string message)
        {
            foreach (var c in _clients)
            {
                await c.Value.SendMessageAsync(message);
            }
        }
    }

    public class ClientProxy : IClientProxy
    {
        private readonly WebSocketClient client;
        public ClientProxy(WebSocketClient client)
        {
            this.client = client;
        }
        public async Task SendMessageAsync(string message)
        {
            await client.SendMessageAsync(message);
        }
    }
}