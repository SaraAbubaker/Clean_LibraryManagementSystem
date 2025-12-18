
using Library.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.Book
{
    public class CreateBookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;
        [Required]
        public DateOnly PublishDate { get; set; }
        [StringLength(50)]
        public string? Version { get; set; }


        [Positive]
        public int? PublisherId { get; set; }

        [Positive]
        public int? AuthorId { get; set; }

        [Positive]
        public int? CategoryId { get; set; }
    }
}
