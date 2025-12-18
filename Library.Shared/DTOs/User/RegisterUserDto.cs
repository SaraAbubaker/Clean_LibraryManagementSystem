
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.User
{
    public class RegisterUserDto
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$",
        ErrorMessage = "Username must contain letters, numbers, underscore, dot and dash only.")]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters, contain upper, lower case, and a number.")]
        public string Password { get; set; } = null!;

    }
}
