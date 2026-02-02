using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Publisher
{
    public class CreatePublisherDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;

    }
}
