using AutoMapper;
using TodoAPI.Controllers.Resources;
using TodoAPI.Core.Models;

namespace TodoAPI.Mapping
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            CreateMap<TodoItem, TodoItemResource>();
            CreateMap(typeof(Result<>), typeof(ResultResource<>));
            CreateMap<TodoItemResource, TodoItem>();
            CreateMap<TodoQueryResourse, TodoQuery>();
        }
    }
}