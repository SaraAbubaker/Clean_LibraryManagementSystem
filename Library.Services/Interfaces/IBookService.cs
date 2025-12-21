using Library.Entities.Models;
using Library.Shared.DTOs;
using Library.Shared.DTOs.Book;

namespace Library.Services.Interfaces
{
    public interface IBookService
    {
        Task<Book> CreateBookAsync(CreateBookDto dto, int currentUserId);
        IQueryable<BookListDto?> GetBookDetailsQuery(int bookId);
        IQueryable<BookListDto> GetBooksByAuthorQuery(int authorId);
        IQueryable<BookListDto> GetBooksByCategoryQuery(int categoryId);
        Task<bool> UpdateBookAsync(UpdateBookDto dto, int currentUserId);
        Task<bool> ArchiveBookAsync(int bookId, int performedByUserId);
        Task<PagedResultDto<BookListDto>> SearchBooksQuery(SearchBookParamsDto dto, SearchParamsDto searchDto);
    }
}
