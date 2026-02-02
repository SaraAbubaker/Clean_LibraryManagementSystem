
using Microsoft.EntityFrameworkCore;
using Library.Shared.DTOs.Category;
using Library.Domain.Repositories;
using Library.Services.Interfaces;
using Library.Entities.Models;
using Mapster;
using Library.Common.Helpers;
using Library.Common.Exceptions;
using Library.Common.DTOs.LibraryDtos.Category;

namespace Library.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<Book> _bookRepo;

        public CategoryService(
            IGenericRepository<Category> categoryRepo,
            IGenericRepository<Book> bookRepo)
        {
            _categoryRepo = categoryRepo;
            _bookRepo = bookRepo;
        }


        //CRUD
        public async Task<CategoryListDto> CreateCategoryAsync(CreateCategoryDto dto, int userId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(userId, nameof(userId));

            var category = dto.Adapt<Category>();

            await _categoryRepo.AddAsync(category, userId);
            await _categoryRepo.CommitAsync();

            return category.Adapt<CategoryListDto>();
        }

        public IQueryable<CategoryListDto> GetAllCategoriesQuery()
        {
            return _categoryRepo.GetAll()
                .AsNoTracking()
                .Select(c => c.Adapt<CategoryListDto>());
        }

        public IQueryable<CategoryListDto> GetCategoryByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _categoryRepo.GetById(id)
                .AsNoTracking()
                .Select(c => c.Adapt<CategoryListDto>());
        }

        public async Task<CategoryListDto> UpdateCategoryAsync(UpdateCategoryDto dto, int userId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(userId, nameof(userId));

            var category = Validate.Exists(
                await _categoryRepo.GetById(dto.Id).FirstOrDefaultAsync(),
                dto.Id
            );

            category.Name = dto.Name;
            await _categoryRepo.UpdateAsync(category, userId);
            await _categoryRepo.CommitAsync();

            return category.Adapt<CategoryListDto>();
        }

        //Archives category & Moves books out of it
        public async Task<bool> ArchiveCategoryAsync(int id, int performedByUserId)
        {
            Validate.Positive(id, nameof(id));

            var category = Validate.Exists(
                await _categoryRepo.GetById(id).FirstOrDefaultAsync(),
                id
            );

            if (category.IsArchived)
                throw new ConflictException($"Category with id {id} is already archived.");

            var books = _bookRepo.GetAll().Where(b => b.CategoryId == id);

            var unknownCategory = await _categoryRepo.GetById(-1).FirstOrDefaultAsync();

            foreach (var book in books)
            {
                book.CategoryId = -1; //Unknown
                book.Category = unknownCategory;
                await _bookRepo.UpdateAsync(book, performedByUserId);
            }

            await _categoryRepo.ArchiveAsync(category, performedByUserId);
            await _categoryRepo.CommitAsync();

            return true;
        }
    }
}
