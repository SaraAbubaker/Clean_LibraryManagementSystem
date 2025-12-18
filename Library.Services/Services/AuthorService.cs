
using Library.Infrastructure.Logging.Interfaces;
using System.ComponentModel.DataAnnotations;
using Library.Infrastructure.Logging.Models;
using Microsoft.EntityFrameworkCore;
using Library.Services.Interfaces;
using Library.Domain.Repositories;
using Library.Shared.DTOs.Author;
using Library.Shared.Exceptions;
using Library.Entities.Models;
using Library.Shared.Helpers;
using Mapster;

namespace Library.Services.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IGenericRepository<Author> _authorRepo;
        private readonly IGenericRepository<Book> _bookRepo;
        private readonly IMessageLoggerService _messageLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public AuthorService(
            IGenericRepository<Author> authorRepo, IGenericRepository<Book> bookRepo,
            IMessageLoggerService messageLogger, IExceptionLoggerService exceptionLogger)
        {
            _authorRepo = authorRepo;
            _bookRepo = bookRepo;
            _messageLogger = messageLogger;
            _exceptionLogger = exceptionLogger;
        }

        //CRUD
        public async Task<AuthorListDto> CreateAuthorAsync(CreateAuthorDto dto, int userId)
        {
            //Log request
            await _messageLogger.LogMessageAsync($"User {userId} is creating author '{dto.Name}'");

            try
            {
                try
                {
                    Validate.ValidateModel(dto);
                    Validate.Positive(userId, nameof(userId));
                }
                catch (Exception ex)
                {
                    // Log validation failure as a warning
                    await LogWarningAsync($"Validation failed for creating author '{dto.Name}'", ex.Message);
                    throw; // Rethrow to propagate the error
                }

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


                //Log response
                await _messageLogger.LogMessageAsync(new MessageLog
                {
                    Guid = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Request = $"User {userId} is creating author '{dto.Name}'",
                    Response = $"Author '{dto.Name}' created successfully",
                    ServiceName = nameof(AuthorService),
                    Level = LogLevel.Info
                });

                return result;
            }
            catch (Exception ex)
            {
                //Log any exception (including validation if not already caught)
                await _exceptionLogger.LogExceptionAsync(ex, nameof(AuthorService));
                throw;
            }
        }

        public IQueryable<AuthorListDto> ListAuthorsQuery()
        {
            //Log request
            _ = _messageLogger.LogMessageAsync(new MessageLog
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Request = $"ListAuthorsQuery was called",
                ServiceName = nameof(AuthorService),
                Level = LogLevel.Request
            });

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
            //Log request
            _ = _messageLogger.LogMessageAsync(new MessageLog
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Request = $"GetAuthorByIdQuery called for id={id}",
                ServiceName = nameof(AuthorService),
                Level = LogLevel.Request
            });

            try
            {
                Validate.Positive(id, nameof(id));
            }
            catch (Exception ex)
            {
                _ = _messageLogger.LogMessageAsync(new MessageLog
                {
                    Guid = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Request = $"Validation failed for GetAuthorByIdQuery with id={id}",
                    Response = ex.Message,
                    ServiceName = nameof(AuthorService),
                    Level = LogLevel.Warning
                });
                throw;
            }

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
            Validate.Positive(userId, nameof(userId));

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
            Validate.Positive(id, nameof(id));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

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
