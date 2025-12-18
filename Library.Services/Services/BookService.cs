using Library.Domain.Repositories;
using Library.Entities.Models;
using Library.Services.Interfaces;
using Library.Shared.DTOs;
using Library.Shared.DTOs.Book;
using Library.Shared.Helpers;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Library.Services.Services
{
    public class BookService : IBookService
    {
        private readonly IGenericRepository<Book> _bookRepo;
        private readonly IGenericRepository<InventoryRecord> _inventoryRepo;

        public BookService(IGenericRepository<Book> bookRepo,
            IGenericRepository<InventoryRecord> inventoryRepo)
        {
            _bookRepo = bookRepo;
            _inventoryRepo = inventoryRepo;
        }

        //CRUD
        public async Task<Book> CreateBookAsync(CreateBookDto dto, int currentUserId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(currentUserId, nameof(currentUserId));

            var book = dto.Adapt<Book>();

            //Generate first copy code from title (e.g., "Harry Potter" -> "HP-01")
            string prefix = CopyCodeGeneratorHelper.GenerateBookPrefix(book.Title);
            string copyCode = $"{prefix}-{1:00}";

            var defaultCopy = new InventoryRecord
            {
                CopyCode = copyCode,
                IsAvailable = true,
                PublisherId = book.PublisherId > 0 ? book.PublisherId : 0,
                CreatedByUserId = currentUserId,
                CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                LastModifiedByUserId = currentUserId,
                LastModifiedDate = DateOnly.FromDateTime(DateTime.Now),
                IsArchived = false
            };

            book.InventoryRecords.Add(defaultCopy);

            await _bookRepo.AddAsync(book, currentUserId);
            await _bookRepo.CommitAsync();

            return book;
        }

        public IQueryable<BookListDto?> GetBookDetailsQuery(int bookId)
        {
            Validate.Positive(bookId, nameof(bookId));

            return _bookRepo.GetAll()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.InventoryRecords)
                .AsNoTracking()
                .Where(b => b.Id == bookId)
                .Select(b => new BookListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublishDate = b.PublishDate,
                    AuthorName = b.Author != null ? b.Author.Name : "Unknown",
                    CategoryName = b.Category != null ? b.Category.Name : "Unknown",
                    PublisherName = b.Publisher != null ? b.Publisher.Name : "Unknown",
                    IsAvailable = b.InventoryRecords.Any(r => r.IsAvailable)
                });
        }

        public IQueryable<BookListDto> GetBooksByAuthorQuery(int authorId)
        {
            Validate.Positive(authorId, nameof(authorId));

            return _bookRepo.GetAll()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.InventoryRecords)
                .AsNoTracking()
                .Where(b => b.AuthorId == authorId)
                .Select(b => new BookListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublishDate = b.PublishDate,
                    AuthorName = b.Author != null ? b.Author.Name : "Unknown",
                    CategoryName = b.Category != null ? b.Category.Name : "Unknown",
                    PublisherName = b.Publisher != null ? b.Publisher.Name : "Unknown",
                    IsAvailable = b.InventoryRecords.Any(r => r.IsAvailable)
                })
                .OrderBy(b => b.AuthorName);
        }

        public IQueryable<BookListDto> GetBooksByCategoryQuery(int categoryId)
        {
            Validate.Positive(categoryId, nameof(categoryId));

            return _bookRepo.GetAll()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.InventoryRecords)
                .AsNoTracking()
                .Where(b => b.CategoryId == categoryId)
                .Select(b => new BookListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublishDate = b.PublishDate,
                    AuthorName = b.Author != null ? b.Author.Name : "Unknown",
                    CategoryName = b.Category != null ? b.Category.Name : "Unknown",
                    PublisherName = b.Publisher != null ? b.Publisher.Name : "Unknown",
                    IsAvailable = b.InventoryRecords.Any(r => r.IsAvailable)
                });
        }

        public async Task<bool> UpdateBookAsync(UpdateBookDto dto, int currentUserId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(currentUserId, nameof(currentUserId));

            var book = Validate.Exists(
                await _bookRepo.GetAll()
                    .FirstOrDefaultAsync(b => b.Id == dto.Id),
                dto.Id
            );

            if (!string.IsNullOrWhiteSpace(dto.Title)) book.Title = dto.Title;
            if (dto.PublishDate.HasValue) book.PublishDate = dto.PublishDate.Value;
            if (dto.Version != null) book.Version = dto.Version;

            if (dto.PublisherId.HasValue) book.PublisherId = dto.PublisherId.Value;
            if (dto.AuthorId.HasValue) book.AuthorId = dto.AuthorId.Value;
            if (dto.CategoryId.HasValue) book.CategoryId = dto.CategoryId.Value;

            await _bookRepo.UpdateAsync(book, currentUserId);
            await _bookRepo.CommitAsync();

            return true;
        }

        public async Task<bool> ArchiveBookAsync(int bookId, int performedByUserId)
        {
            Validate.Positive(bookId, nameof(bookId));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var book = Validate.Exists(
                await _bookRepo.GetAll()
                    .FirstOrDefaultAsync(b => b.Id == bookId),
                bookId
            );

            await _bookRepo.ArchiveAsync(book, performedByUserId);
            await _bookRepo.CommitAsync();
            return true;
        }

        //Search method (filter, sort, pagination)
        public async Task<PagedResult<BookListDto>> SearchBooksQuery(SearchBookParamsDto dto, SearchParamsDto searchDto)
        {
            Validate.ValidateModel(dto);
            Validate.ValidateModel(searchDto);

            int page = Math.Max(1, searchDto.Page);
            int pageSize = Math.Max(1, searchDto.PageSize);
            int skip = (page - 1) * pageSize;

            var books = _bookRepo.GetAll()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.InventoryRecords)
                .AsNoTracking();

            //Filter
            if (!string.IsNullOrWhiteSpace(dto.Title))
                books = books.Where(b => b.Title != null && b.Title.Contains(dto.Title.Trim()));

            if (!string.IsNullOrWhiteSpace(dto.AuthorName))
                books = books.Where(b => b.Author != null && b.Author.Name.Contains(dto.AuthorName.Trim()));

            if (!string.IsNullOrWhiteSpace(dto.CategoryName))
                books = books.Where(b => b.Category != null && b.Category.Name.Contains(dto.CategoryName.Trim()));

            if (!string.IsNullOrWhiteSpace(dto.PublisherName))
                books = books.Where(b => b.Publisher != null && b.Publisher.Name.Contains(dto.PublisherName.Trim()));

            if (!string.IsNullOrWhiteSpace(dto.PublishYearOrDate))
            {
                var value = dto.PublishYearOrDate.Trim();

                if (int.TryParse(value, out int year))
                    books = books.Where(b => b.PublishDate.Year == year);
                else if (DateOnly.TryParse(value, out DateOnly date))
                    books = books.Where(b => b.PublishDate == date);
                else
                    throw new ValidationException("PublishYearOrDate must be a valid year or full date (YYYY-MM-DD).");
            }

            //Search
            if (!string.IsNullOrWhiteSpace(searchDto.SearchParam))
            {
                var search = searchDto.SearchParam.Trim();
                bool isYear = int.TryParse(search, out int searchYear);
                bool isDate = DateOnly.TryParse(search, out DateOnly searchDate);

                books = books.Where(b =>
                    (b.Title != null && b.Title.Contains(search)) ||
                    (b.Author != null && b.Author.Name.Contains(search)) ||
                    (b.Category != null && b.Category.Name.Contains(search)) ||
                    (b.Publisher != null && b.Publisher.Name.Contains(search)) ||
                    (isYear && b.PublishDate.Year == searchYear) ||
                    (isDate && b.PublishDate == searchDate)
                );
            }

            //Sorting
            books = searchDto.SortBy switch
            {
                SearchParamsDto.BookSortBy.Id => searchDto.SortDir == SearchParamsDto.SortDirection.Desc
                    ? books.OrderByDescending(b => b.Id)
                    : books.OrderBy(b => b.Id),

                SearchParamsDto.BookSortBy.Title => searchDto.SortDir == SearchParamsDto.SortDirection.Desc
                    ? books.OrderByDescending(b => b.Title ?? "")
                    : books.OrderBy(b => b.Title ?? ""),

                SearchParamsDto.BookSortBy.PublishDate => searchDto.SortDir == SearchParamsDto.SortDirection.Desc
                    ? books.OrderByDescending(b => b.PublishDate)
                    : books.OrderBy(b => b.PublishDate),

                SearchParamsDto.BookSortBy.Author => searchDto.SortDir == SearchParamsDto.SortDirection.Desc
                    ? books.OrderByDescending(b => b.Author != null ? b.Author.Name : "")
                    : books.OrderBy(b => b.Author != null ? b.Author.Name : ""),

                SearchParamsDto.BookSortBy.Category => searchDto.SortDir == SearchParamsDto.SortDirection.Desc
                    ? books.OrderByDescending(b => b.Category != null ? b.Category.Name : "")
                    : books.OrderBy(b => b.Category != null ? b.Category.Name : ""),

                SearchParamsDto.BookSortBy.Publisher => searchDto.SortDir == SearchParamsDto.SortDirection.Desc
                    ? books.OrderByDescending(b => b.Publisher != null ? b.Publisher.Name : "")
                    : books.OrderBy(b => b.Publisher != null ? b.Publisher.Name : ""),

                _ => books.OrderBy(b => b.Id)
            };

            int totalCount = await books.CountAsync();

            //Pagination + mapping
            var pagedItems = await books
                .Skip(skip)
                .Take(pageSize)
                .Select(b => new BookListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublishDate = b.PublishDate,
                    AuthorName = b.Author != null ? b.Author.Name : "Unknown",
                    CategoryName = b.Category != null ? b.Category.Name : "Unknown",
                    PublisherName = b.Publisher != null ? b.Publisher.Name : "Unknown",
                    IsAvailable = b.InventoryRecords.Any(r => r.IsAvailable)
                })
                .ToListAsync();

            return new PagedResult<BookListDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
