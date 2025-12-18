
using Library.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.UserType
{
    public class UpdateUserTypeDto
    {
        [Required]
        [Positive]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Role { get; set; } = null!;

    }
}
