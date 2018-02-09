using System.Collections.Generic;

namespace TodoAPI.Middleware
{
    public interface IHubClients
    {
        IClientProxy All { get; }
        IClientProxy AllExecpt(IEnumerable<string> excludeIds);
        IClientProxy Client(string clientId);
    }
}