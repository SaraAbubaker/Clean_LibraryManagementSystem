
using Library.Shared.DTOs.Author;

namespace Library.Services.Interfaces
{
    public interface IAuthorService
    {
        Task<AuthorListDto> CreateAuthorAsync(CreateAuthorDto dto, int userId);
        IQueryable<AuthorListDto> ListAuthorsQuery();
        IQueryable<AuthorListDto?> GetAuthorByIdQuery(int id);
        Task<bool> EditAuthorAsync(UpdateAuthorDto dto, int userId);
        Task<bool> ArchiveAuthorAsync(int id, int performedByUserId);
    }
}

