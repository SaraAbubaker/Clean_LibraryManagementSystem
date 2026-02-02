using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Book
{
    public class SearchBookParamsDto
    {
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }
        public string? PublishYearOrDate { get; set; }


        [MaxLength(150, ErrorMessage = "Author name cannot exceed 150 characters.")]
        public string? AuthorName { get; set; }


        [MaxLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        public string? CategoryName { get; set; }


        [MaxLength(50, ErrorMessage = "Publisher name cannot exceed 50 characters.")]
        public string? PublisherName { get; set; }

    }
}
