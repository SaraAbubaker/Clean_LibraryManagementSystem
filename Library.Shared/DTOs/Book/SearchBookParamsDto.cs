
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.Book
{
    public class SearchBookParamsDto
    {
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string? Title { get; set; }
        public string? PublishYearOrDate { get; set; }


        [MaxLength(50, ErrorMessage = "Author name cannot exceed 50 characters.")]
        public string? AuthorName { get; set; }


        [MaxLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        public string? CategoryName { get; set; }


        [MaxLength(50, ErrorMessage = "Publisher name cannot exceed 50 characters.")]
        public string? PublisherName { get; set; }

    }
}
