using System.Collections.Generic;
using System.Threading.Tasks;
using TodoAPI.Core.Models;

namespace TodoAPI.Core
{
    public interface ITodoRepository
    {
        Task<Result<TodoItem>> GetTodos(TodoQuery query, User user);
        Task<TodoItem> GetTodo(long id, User user);

        void Add(TodoItem item);

        void Remove(TodoItem item);
    }
}