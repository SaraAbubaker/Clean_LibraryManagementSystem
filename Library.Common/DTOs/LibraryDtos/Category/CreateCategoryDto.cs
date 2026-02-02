using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Category
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; } = null!;
    }
}
