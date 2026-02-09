using Library.Common.DTOs.LibraryDtos.Book;

namespace Library.UI.Models
{
    public class BookViewModel
    {
        public List<BookListDto> Books { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; } = 5;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public string? Search { get; set; }
        public string? Filter { get; set; }
    }
}