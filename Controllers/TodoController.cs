using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoAPI.Controllers.Resources;
using TodoAPI.Core;
using TodoAPI.Core.Models;

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    public class TodoController : Controller
    {
        private readonly ITodoRepository repository;
        private readonly IUnitOfWork unit;
        private readonly IMapper mapper;
        public TodoController(ITodoRepository repository, IUnitOfWork unit, IMapper mapper)
        {
            this.mapper = mapper;
            this.unit = unit;
            this.repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(TodoQueryResourse query)
        {
            var q = mapper.Map<TodoQuery>(query);
            var items = await repository.GetTodos(q);
            var resource = mapper.Map<ResultResource<TodoItemResource>>(items);
            return Ok(resource);
        }

        [HttpGet("{id}", Name = "GetTodo")]
        public async Task<IActionResult> GetById(long id)
        {
            var item = await repository.GetTodo(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<TodoItemResource>(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TodoItemResource item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            repository.Add(mapper.Map<TodoItem>(item));
            await unit.CompleteAsync();

            return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] TodoItemResource item)
        {
            if (item == null || item.Id != id)
            {
                return BadRequest();
            }

            var todo = await repository.GetTodo(id);
            if (todo == null)
            {
                return NotFound();
            }

            mapper.Map<TodoItemResource, TodoItem>(item, todo);

            await unit.CompleteAsync();

            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var todo = await repository.GetTodo(id);
            if (todo == null)
            {
                return NotFound();
            }

            repository.Remove(todo);
            await unit.CompleteAsync();
            return new NoContentResult();
        }
    }
}