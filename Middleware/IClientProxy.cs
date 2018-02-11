using System.Threading.Tasks;

namespace TodoAPI.Middleware
{
    public interface IClientProxy
    {
        Task SendMessageAsync(string action, object message);
    }
}