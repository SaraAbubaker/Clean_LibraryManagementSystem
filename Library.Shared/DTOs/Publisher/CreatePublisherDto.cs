
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.Publisher
{
    public class CreatePublisherDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;

    }
}
