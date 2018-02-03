using System.Threading.Tasks;
using TodoAPI.Core.Models;

namespace TodoAPI.Core
{
    public interface IUserRepository
    {
        void Add(User user);
        Task Confirm(string code);
        Task<User> FindByName(string username);
        Task<User> FindByEmail(string email);
    }
}