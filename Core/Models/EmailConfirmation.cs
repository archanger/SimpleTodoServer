using System;

namespace TodoAPI.Core.Models
{
    public class EmailConfirmation
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ConfiramtionCode { get; set; }
        public bool Confirmed { get; set; } = false;
    }
}