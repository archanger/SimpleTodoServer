using AutoMapper;
using TodoAPI.Controllers.Resources;
using TodoAPI.Core.Models;
using TodoAPI.Helpers;

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
            CreateMap<AccountRegisterResource, User>()
                .ForMember(u => u.PasswordHash, opt => opt.MapFrom(a => Hashing.HashPassword(a.Password)));
        }
    }
}