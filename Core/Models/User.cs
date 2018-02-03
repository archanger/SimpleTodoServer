using System;
using System.Collections.Generic;

namespace TodoAPI.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public EmailConfirmation EmailConfirmation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public IEnumerable<TodoItem> Items { get; set; }
    }
}