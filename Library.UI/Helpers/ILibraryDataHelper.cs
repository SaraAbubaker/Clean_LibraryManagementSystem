using Library.Common.DTOs.LibraryDtos.Author;
using Library.Common.DTOs.LibraryDtos.Category;
using Library.Common.DTOs.LibraryDtos.Publisher;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.UI.Helpers
{
    public interface ILibraryDataHelper
    {
        Task<List<CategoryListDto>> GetCategoriesAsync();
        Task<List<PublisherListDto>> GetPublishersAsync();
        Task<List<AuthorListDto>> GetAuthorsAsync();

        Task<SelectList> GetCategoriesSelectListAsync(int? selectedId = null);
        Task<SelectList> GetPublishersSelectListAsync(int? selectedId = null);
        Task<SelectList> GetAuthorsSelectListAsync(int? selectedId = null);
    }
}
