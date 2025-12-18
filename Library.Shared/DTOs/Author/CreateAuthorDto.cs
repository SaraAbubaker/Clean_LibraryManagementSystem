
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.Author
{
    public class CreateAuthorDto
    {
        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Author name must be between 2 and 100 characters.")]
        public string Name { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

    }
}
