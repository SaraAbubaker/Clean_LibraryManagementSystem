
using Microsoft.EntityFrameworkCore;
using Library.Services.Interfaces;
using Library.Domain.Repositories;
using Library.Entities.Models;
using Library.Common.Helpers;
using Library.Common.Exceptions;
using Library.Common.DTOs.LibraryDtos.BorrowRecord;

namespace Library.Services.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly IGenericRepository<BorrowRecord> _borrowRepo;
        private readonly IGenericRepository<InventoryRecord> _inventoryRepo;
        private readonly IInventoryService _inventoryService;

        public BorrowService(
            IGenericRepository<BorrowRecord> borrowRepo,
            IGenericRepository<InventoryRecord> inventoryRepo,
            IInventoryService inventoryService)
        {
            _borrowRepo = borrowRepo;
            _inventoryRepo = inventoryRepo;
            _inventoryService = inventoryService;
        }


        //ListAll
        public IQueryable<BorrowListDto> GetBorrowDetailsQuery()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return _borrowRepo.GetAll()
                .AsNoTracking()
                .Include(b => b.InventoryRecord)
                .Select(b => new BorrowListDto
                {
                    Id = b.Id,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    CopyCode = b.InventoryRecord != null ? b.InventoryRecord.CopyCode : null,
                    UserId = b.UserId,
                    IsOverdue = b.ReturnDate == null && b.DueDate < today,
                    OverdueDays = EF.Functions.DateDiffDay(
                        b.DueDate.ToDateTime(TimeOnly.MinValue),
                        (b.ReturnDate ?? today).ToDateTime(TimeOnly.MinValue)
                        ) > 0 ? EF.Functions.DateDiffDay(
                            b.DueDate.ToDateTime(TimeOnly.MinValue),
                            (b.ReturnDate ?? today).ToDateTime(TimeOnly.MinValue)
                            ) : 0
                });
        }

        //Availability
        public async Task<bool> HasAvailableCopyAsync(int bookId)
        {
            ValidationHelpers.ValidatePositive(bookId, nameof(bookId));
            return _inventoryService.GetAvailableCopiesQuery(bookId).Any();
        }

        public IQueryable<InventoryRecord> GetAvailableCopiesQuery(int bookId)
        {
            ValidationHelpers.ValidatePositive(bookId, nameof(bookId));

            return _inventoryRepo.GetAll()
                .AsNoTracking()
                .Where(ir => ir.BookId == bookId && ir.IsAvailable);
        }

        //Borrow & Return
        public async Task<BorrowResponseDto> BorrowBookAsync(RequestBorrowDto dto, int userId)
        {
            Validate.ValidateModel(dto);
            ValidationHelpers.ValidatePositive(userId, nameof(userId));

            var copy = await _inventoryService
                .GetAvailableCopiesQuery(dto.BookId)
                .FirstOrDefaultAsync();

            if (copy == null)
                throw new NotFoundException($"No available copies for BookId {dto.BookId}");

            copy.IsAvailable = false;
            await _inventoryRepo.UpdateAsync(copy, userId);

            var borrow = new BorrowRecord
            {
                InventoryRecordId = copy.Id,
                UserId = userId,
                BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = dto.DueDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
                ReturnDate = null,
            };

            await _borrowRepo.AddAsync(borrow, userId);
            await _borrowRepo.CommitAsync();

            var response = new BorrowResponseDto
            {
                BorrowRecordId = borrow.Id,
                InventoryRecordId = borrow.InventoryRecordId,
                BorrowDate = borrow.BorrowDate,
                DueDate = borrow.DueDate
            };

            return response;
        }

        public async Task<bool> ReturnBookAsync(int borrowRecordId, int currentUserId)
        {
            ValidationHelpers.ValidatePositive(borrowRecordId, nameof(borrowRecordId));
            ValidationHelpers.ValidatePositive(currentUserId, nameof(currentUserId));

            var record = Validate.Exists(
                await _borrowRepo.GetById(borrowRecordId).FirstOrDefaultAsync(),
                borrowRecordId
            );

            if (record.ReturnDate != null)
                throw new ConflictException($"Borrow record with id {borrowRecordId} has already been returned.");

            record.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
            await _borrowRepo.UpdateAsync(record, currentUserId);
            await _borrowRepo.CommitAsync();

            return await _inventoryService.ReturnCopyAsync(record.InventoryRecordId, currentUserId);
        }



        //Overdue Logic
        public IQueryable<BorrowRecord> GetOverdueRecordsQuery()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return _borrowRepo.GetAll()
                .AsNoTracking()
                .Where(r => r.ReturnDate == null && r.DueDate < today);
        }

        public bool IsBorrowOverdue(BorrowRecord record)
        {
            Validate.NotNull(record, nameof(record));

            if (record.ReturnDate != null) return false;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return today > record.DueDate;
        }

        public int CalculateOverdueDays(BorrowRecord record)
        {
            Validate.NotNull(record, nameof(record));

            var endDate = record.ReturnDate ?? DateOnly.FromDateTime(DateTime.Today);
            var dueDate = record.DueDate;

            if (endDate <= dueDate) return 0;

            var days = (endDate.ToDateTime(TimeOnly.MinValue) - dueDate.ToDateTime(TimeOnly.MinValue)).Days;
            return Math.Max(0, days);
        }
    }
}