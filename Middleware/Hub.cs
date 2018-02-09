using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoAPI.Middleware
{
  public abstract class Hub
  {
    public IHubClients Clients { get; set; }
    abstract public Task OnConnected(string clientId);

    abstract public Task OnDisconnected(string clientId);

    abstract public Task MessageReceived(string clientId, string message);
  }
}