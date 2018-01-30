using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoAPI.Core;
using TodoAPI.Core.Models;
using TodoAPI.Persistance;
using System.Linq;

namespace TodoAPI.Persistance
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoContext context;
        public TodoRepository(TodoContext context)
        {
            this.context = context;
        }

        public async Task<Result<TodoItem>> GetTodos(TodoQuery query)
        {
            var result = new Result<TodoItem>();

            var q = context.TodoItems.AsQueryable();
            if (query.Limit <= 0) {
                query.Limit = 10;
            } 
            if (query.Offset <= 0) {
                query.Offset = 0;
            }
            q = q.Skip(query.Offset).Take(query.Limit);

            result.data = await q.ToListAsync();
            result.TotalItems = await q.CountAsync();
            return result;
        }

        public void Add(TodoItem item)
        {
            context.TodoItems.Add(item);
        }

        public void Remove(TodoItem item)
        {
            context.TodoItems.Remove(item);
        }

        public async Task<TodoItem> GetTodo(long id)
        {
            return await context.TodoItems.FindAsync(id);
        }
    }
}