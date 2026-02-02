using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Publisher
{
    public class UpdatePublisherDto
    {
        [Required]
        [Positive]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;

    }
}
