using System.ComponentModel.DataAnnotations;

namespace TodoAPI.Controllers.Resources
{
    public class AccountRegisterResource
    {
        [Required]
        [StringLength(128, MinimumLength = 3)]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "The password must be at least 6 symbols length")]
        public string Password { get; set; }
    }
}