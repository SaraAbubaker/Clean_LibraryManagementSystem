
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.User
{
    public class LoginDto
    {
        [Required]
        public string UsernameOrEmail { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
