using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Category
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = null!;
    }
}
