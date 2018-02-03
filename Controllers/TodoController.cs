using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IUserRepository userRepository;
        public TodoController(ITodoRepository repository, IUserRepository userRepository, IUnitOfWork unit, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.unit = unit;
            this.repository = repository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(TodoQueryResourse query)
        {
            var user = await GetUser();
            if (user == null) {
                return Unauthorized();
            }

            var q = mapper.Map<TodoQuery>(query);
            var items = await repository.GetTodos(q, user);
            var resource = mapper.Map<ResultResource<TodoItemResource>>(items);
            return Ok(resource);
        }

        [Authorize]
        [HttpGet("{id}", Name = "GetTodo")]
        public async Task<IActionResult> GetById(long id)
        {
            var user = await GetUser();
            if (user == null) {
                return Unauthorized();
            }
            var item = await repository.GetTodo(id, user);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<TodoItemResource>(item));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] TodoItemResource item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var user = await GetUser();
            if (user == null) {
                return Unauthorized();
            }

            var todoItem = mapper.Map<TodoItem>(item);
            todoItem.User = user;
            repository.Add(todoItem);
            await unit.CompleteAsync();

            return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] TodoItemResource item)
        {
            if (item == null || item.Id != id)
            {
                return BadRequest();
            }

            var user = await GetUser();
            if (user == null) {
                return Unauthorized();
            }

            var todo = await repository.GetTodo(id, user);
            if (todo == null)
            {
                return NotFound();
            }

            mapper.Map<TodoItemResource, TodoItem>(item, todo);

            await unit.CompleteAsync();

            return new NoContentResult();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var user = await GetUser();
            if (user == null) {
                return Unauthorized();
            }
            var todo = await repository.GetTodo(id, user);
            if (todo == null)
            {
                return NotFound();
            }

            repository.Remove(todo);
            await unit.CompleteAsync();
            return new NoContentResult();
        }

        private async Task<User> GetUser()
        {
            var guid = ExtractGuidFromUser();
            if (!guid.HasValue)
            {
                return null;
            }

            var user = await userRepository.FindById(guid.Value);
            if (user == null) {
                return null;
            }

            return user;
        }
        private Guid? ExtractGuidFromUser()
        {
            var user = HttpContext.User;
            var id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (id == null)
            {
                return null;
            }

            return Guid.Parse(id.Value);
        }
    }
}