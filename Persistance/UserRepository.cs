using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoAPI.Core;
using TodoAPI.Core.Models;

namespace TodoAPI.Persistance
{
    public class UserRepository : IUserRepository
    {
        public TodoContext Context { get; }
        public UserRepository(TodoContext context)
        {
            this.Context = context;
        }

        public void Add(User user)
        {
            var conf = new EmailConfirmation { UserId = user.Id, ConfiramtionCode = Guid.NewGuid() };
            user.EmailConfirmation = conf;
            Context.Users.Add(user);
        }

        public async Task<User> FindById(Guid id)
        {
            return await Context.Users.FindAsync(id);
        }

        public async Task<bool> IsConfirmed(string confirmationCode) 
        {
            var guid = Guid.Parse(confirmationCode);
            var confirmation = await Context.EmailConfirmations.FirstOrDefaultAsync(c => c.ConfiramtionCode == guid);
            return confirmation.Confirmed;
        }

        public async Task<User> FindByName(string username)
        {
            return await Context.Users.SingleOrDefaultAsync( u => u.Username == username);
        }

        public async Task<User> FindByEmail(string email)
        {
            return await Context.Users.SingleOrDefaultAsync( u => u.Email == email);
        }

        public async Task Confirm(string code)
        {
            var confirmation = await Context.EmailConfirmations.FirstAsync(
                c => c.ConfiramtionCode == Guid.Parse(code)
            );

            confirmation.Confirmed = true;

        }
    }
}