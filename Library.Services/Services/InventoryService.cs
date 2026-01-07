
using Microsoft.EntityFrameworkCore;
using Library.Services.Interfaces;
using Library.Domain.Repositories;
using Library.Entities.Models;
using Library.Shared.Helpers;

namespace Library.Services.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IGenericRepository<InventoryRecord> _inventoryRepo;
        private readonly IGenericRepository<Book> _bookRepo;
        private readonly IBookService _bookService;

        public InventoryService(
            IGenericRepository<InventoryRecord> inventoryRepo,
            IGenericRepository<Book> bookRepo,
            IBookService bookService)
        {
            _inventoryRepo = inventoryRepo;
            _bookRepo = bookRepo;
            _bookService = bookService;
        }

        //Create, Remove, Read
        public async Task<InventoryRecord> CreateCopyAsync(int bookId, int createdByUserId)
        {
            Validate.Positive(bookId, nameof(bookId));
            Validate.Positive(createdByUserId, nameof(createdByUserId));

            var book = Validate.Exists(
                await _bookRepo
                    .GetById(bookId)
                    .Include(b => b.InventoryRecords)
                    .FirstOrDefaultAsync(),
                bookId
            );

            string prefix = CopyCodeGeneratorHelper.GenerateBookPrefix(book.Title);

            int nextNumber = book.InventoryRecords.Any()
                ? book.InventoryRecords.Count + 1
                : 1;

            string copyCode = $"{prefix}-{nextNumber:00}";

            var record = new InventoryRecord
            {
                BookId = bookId,
                CopyCode = copyCode,
                IsAvailable = true,
                PublisherId = book.PublisherId,
                CreatedByUserId = createdByUserId,
                CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                LastModifiedByUserId = createdByUserId,
                LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsArchived = false
            };

            await _inventoryRepo.AddAsync(record, createdByUserId);
            await _inventoryRepo.CommitAsync();

            return record;
        }

        public IQueryable<InventoryRecord> ListCopiesForBookQuery(int bookId)
        {
            Validate.Positive(bookId, nameof(bookId));

            return _inventoryRepo.GetAll()
                .AsNoTracking()
                .Where(r => r.BookId == bookId)
                .OrderBy(r => r.Id);
        }

        public IQueryable<InventoryRecord> GetAvailableCopiesQuery(int bookId)
        {
            Validate.Positive(bookId, nameof(bookId));

            return _inventoryRepo.GetAll()
                .AsNoTracking()
                .Where(r => r.BookId == bookId && r.IsAvailable);
        }

        //Available = true + audit fields set
        public async Task<bool> ReturnCopyAsync(int inventoryRecordId, int currentUserId)
        {
            Validate.Positive(inventoryRecordId, nameof(inventoryRecordId));
            Validate.Positive(currentUserId, nameof(currentUserId));

            var copy = Validate.Exists(
                await _inventoryRepo.GetById(inventoryRecordId).FirstOrDefaultAsync(),
                inventoryRecordId
            );

            copy.IsAvailable = true;
            await _inventoryRepo.UpdateAsync(copy, currentUserId);

            return true;
        }

        //Archive
        public async Task<bool> ArchiveCopyAsync(int inventoryRecordId, int performedByUserId)
        {
            Validate.Positive(inventoryRecordId, nameof(inventoryRecordId));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var copy = Validate.Exists(
                await _inventoryRepo.GetById(inventoryRecordId).FirstOrDefaultAsync(),
                inventoryRecordId
            );

            await _inventoryRepo.ArchiveAsync(copy, performedByUserId);

            //Archive the book if no copies remain
            var anyLeft = _inventoryRepo.GetAll()
                .Any(r => r.BookId == copy.BookId);
            if (!anyLeft)
            {
                await _bookService.ArchiveBookAsync(copy.BookId, performedByUserId);
            }

            return true;
        }

    }
}
