using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Author
{
    public class CreateAuthorDto
    {
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Author name must be between 2 and 150 characters.")]
        [Required(ErrorMessage = "Author name is required.")]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }
    }
}