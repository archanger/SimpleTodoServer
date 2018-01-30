using System.Threading.Tasks;
using TodoAPI.Core;

namespace TodoAPI.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TodoContext context;
        public UnitOfWork(TodoContext context)
        {
            this.context = context;
        }
        public async Task CompleteAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}