using System;
using System.Threading.Tasks;
using TodoAPI.Core.Models;

namespace TodoAPI.Core
{
    public interface IUserRepository
    {
        void Add(User user);
        Task Confirm(string code);
        Task<User> FindById(Guid id);
        Task<User> FindByName(string username);
        Task<bool> IsConfirmed(string confirmationCode);
        Task<User> FindByEmail(string email);
    }
}