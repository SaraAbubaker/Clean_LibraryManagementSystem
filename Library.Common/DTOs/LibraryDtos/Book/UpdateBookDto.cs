using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Book
{
    public class UpdateBookDto
    {
        [Required]
        [Positive]
        public int Id { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }
        public DateOnly? PublishDate { get; set; }
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
