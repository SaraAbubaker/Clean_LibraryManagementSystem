
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.UserType
{
    public class CreateUserTypeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Role { get; set; } = null!;

    }
}
