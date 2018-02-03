using Microsoft.EntityFrameworkCore;
using TodoAPI.Core.Models;

namespace TodoAPI.Persistance
{
    public class TodoContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EmailConfirmation> EmailConfirmations { get; set; }

        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
            
        }
    }
}