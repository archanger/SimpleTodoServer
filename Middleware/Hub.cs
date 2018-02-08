using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoAPI.Middleware
{
  public abstract class Hub
  {
    public Clinets Clients { get; }
    public Hub(Clinets clients)
    {
      this.Clients = clients;
    }
    abstract public Task OnConnected(WebSocketClient client);

    abstract public Task OnDisconnected(WebSocketClient client);

    abstract public Task MessageReceived(WebSocketClient client, string message);
  }
}