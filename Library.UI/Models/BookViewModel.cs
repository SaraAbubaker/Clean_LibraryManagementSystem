using Library.Common.DTOs.LibraryDtos.Book;

namespace Library.UI.Models
{
    public class BookViewModel
    {
        public List<BookListDto> Books { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
