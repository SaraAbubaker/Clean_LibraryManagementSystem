
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.Category
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; } = null!;
    }
}
