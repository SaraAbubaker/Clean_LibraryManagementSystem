
using Library.Common.DTOs.LibraryDtos.Category;

namespace Library.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryListDto> CreateCategoryAsync(CreateCategoryDto dto, int userId);
        IQueryable<CategoryListDto> GetAllCategoriesQuery();
        IQueryable<CategoryListDto> GetCategoryByIdQuery(int id);
        Task<CategoryListDto> UpdateCategoryAsync(UpdateCategoryDto dto, int userId);
        Task<bool> ArchiveCategoryAsync(int id, int performedByUserId);
    }
}
