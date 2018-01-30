using System.ComponentModel.DataAnnotations;

namespace TodoAPI.Controllers.Resources
{
    public class TodoItemResource
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
    }
}