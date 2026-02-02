
using Library.Common.DTOs.LibraryDtos.Author;
using Library.Common.Exceptions;
using Library.Common.Helpers;
using Library.Domain.Repositories;
using Library.Entities.Models;
using Library.Services.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Library.Services.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IGenericRepository<Author> _authorRepo;
        private readonly IGenericRepository<Book> _bookRepo;

        public AuthorService(
            IGenericRepository<Author> authorRepo, IGenericRepository<Book> bookRepo)
        {
            _authorRepo = authorRepo;
            _bookRepo = bookRepo;
        }

        //CRUD
        public async Task<AuthorListDto> CreateAuthorAsync(CreateAuthorDto dto, int userId)
        {

            Validate.ValidateModel(dto);
            ValidationHelpers.ValidatePositive(userId, nameof(userId));

            var name = dto.Name.Trim();
            var email = dto.Email?.Trim();

            var nameLower = name.ToLowerInvariant();
            var authorExists = await _authorRepo.GetAll()
                .FirstOrDefaultAsync(a => a.Name.ToLower() == nameLower);

            if (authorExists != null)
                throw new ConflictException($"An author with the name '{name}' already exists.");

            var author = new Author
            {
                Name = dto.Name,
                Email = dto.Email,
                IsArchived = false,
            };

            await _authorRepo.AddAsync(author, userId);
            await _authorRepo.CommitAsync();

            var result = author.Adapt<AuthorListDto>();

            result.BookCount = await _bookRepo.GetAll().CountAsync(b => b.AuthorId == author.Id);


            return result;

        }

        public IQueryable<AuthorListDto> ListAuthorsQuery()
        {

            return _authorRepo.GetAll()
                .Include(a => a.Books)
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .Select(a => new AuthorListDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    BookCount = a.Books.Count
                });
        }

        public IQueryable<AuthorListDto> GetAuthorByIdQuery(int id)
        {
            ValidationHelpers.ValidatePositive(id, nameof(id));


            return _authorRepo.GetAll()
                .Include(a => a.Books)
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AuthorListDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Email = a.Email,
                    BookCount = a.Books.Count
                });
        }

        public async Task<bool> EditAuthorAsync(UpdateAuthorDto dto, int userId)
        {
            Validate.ValidateModel(dto);
            ValidationHelpers.ValidatePositive(userId, nameof(userId));

            var name = (dto.Name ?? string.Empty).Trim();
            Validate.NotEmpty(name, nameof(dto.Name));

            var author = Validate.Exists(
                await _authorRepo.GetAll()
                    .Include(a => a.Books)
                    .FirstOrDefaultAsync(a => a.Id == dto.Id),
                dto.Id
            );

            var nameLower = name.ToLower();
            var exists = await _authorRepo.GetAll()
                .Where(a => a.Id != dto.Id)
                .AnyAsync(a => a.Name.ToLower() == nameLower);

            if (exists)
                throw new ConflictException($"Another author with the name '{name}' already exists.");

            author.Name = name;
            author.Email = dto.Email?.Trim() ?? string.Empty;

            await _authorRepo.UpdateAsync(author, userId);
            await _authorRepo.CommitAsync();

            return true;
        }

        public async Task<bool> ArchiveAuthorAsync(int id, int performedByUserId)
        {
            ValidationHelpers.ValidatePositive(id, nameof(id));
            ValidationHelpers.ValidatePositive(performedByUserId, nameof(performedByUserId));

            var author = Validate.Exists(
                await _authorRepo.GetAll()
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id),
                id
                );

            foreach (var book in author.Books)
            {
                book.AuthorId = -1; // Unknown
                await _bookRepo.UpdateAsync(book, performedByUserId);
            }
            //do we add name & email in loop above?^
            author.Name = "Unknown";
            author.Email = string.Empty;

            await _authorRepo.ArchiveAsync(author, performedByUserId);
            await _authorRepo.CommitAsync();

            return true;
        }
    }
}
